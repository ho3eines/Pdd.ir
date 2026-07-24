using Pdd.ir.Business.Models.DTOs;
using Pdd.ir.Business.Services;
using Pdd.ir.Server.Services;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Pdd.ir.Server.WebSocket
{
    public class WebSocketHandler
    {
        private readonly ConnectionManager _connectionManager;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<WebSocketHandler> _logger;
        private readonly AesKeyStore _keyStore;
        private readonly CryptoJsService _crypto;
        private readonly IConfiguration _config;
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public WebSocketHandler(
            ConnectionManager connectionManager,
            IServiceScopeFactory scopeFactory,
            ILogger<WebSocketHandler> logger,
            AesKeyStore keyStore,
            CryptoJsService crypto,
            IConfiguration config)
        {
            _connectionManager = connectionManager;
            _scopeFactory = scopeFactory;
            _logger = logger;
            _keyStore = keyStore;
            _crypto = crypto;
            _config = config;
        }

        public async Task HandleAsync(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400;
                return;
            }

            // ── Accept WS connection (auth validated per-message via auth field) ──
            var connectionId = Guid.NewGuid().ToString();
            var socket = await context.WebSockets.AcceptWebSocketAsync();
            _connectionManager.AddConnection(connectionId, socket);

            _logger.LogInformation("WS connected: {Id}", connectionId);

            try
            {
                var buffer = new byte[1024 * 64];
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                while (!result.CloseStatus.HasValue)
                {
                    if (result.EndOfMessage)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        _ = ProcessMessageAsync(connectionId, message);
                    }
                    else
                    {
                        await ReceiveFullMessageAsync(socket, buffer, result);
                    }

                    result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }

                await socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
            catch (WebSocketException ex)
            {
                _logger.LogWarning("WS error: {Id} - {Error}", connectionId, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WS unexpected error: {Id}", connectionId);
            }
            finally
            {
                _connectionManager.RemoveConnection(connectionId);
                _logger.LogInformation("WS disconnected: {Id}", connectionId);
            }
        }

        private async Task ReceiveFullMessageAsync(System.Net.WebSockets.WebSocket socket, byte[] buffer, WebSocketReceiveResult firstResult)
        {
            using var ms = new MemoryStream();
            ms.Write(buffer, 0, firstResult.Count);

            while (!firstResult.EndOfMessage)
            {
                firstResult = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                ms.Write(buffer, 0, firstResult.Count);
            }
        }

        private async Task ProcessMessageAsync(string connectionId, string rawMessage)
        {
            try
            {
                var request = JsonSerializer.Deserialize<WsRequest>(rawMessage, JsonOpts);

                if (request == null || string.IsNullOrEmpty(request.Action))
                {
                    await SendAsync(connectionId, new WsResponse
                    {
                        Id = request?.Id,
                        Success = false,
                        Message = "Invalid request: action is required"
                    });
                    return;
                }

                _logger.LogDebug("WS received: {Id} -> {Action}", connectionId, request.Action);

                // ── Decrypt incoming data: try SharedKey first, then any AES key ──
                var sharedKey = _config["ApiKey"] ?? "";
                if (!string.IsNullOrEmpty(request.Data))
                {
                    // Try SharedKey first
                    var decrypted = false;
                    if (!string.IsNullOrEmpty(sharedKey))
                    {
                        try
                        {
                            request.Data = _crypto.Decrypt(sharedKey, request.Data);
                            decrypted = true;
                        }
                        catch { }
                    }

                    // If SharedKey failed, try any AES key from AesKeyStore
                    if (!decrypted)
                    {
                        try
                        {
                            var aesDecrypted = _keyStore.TryDecryptWithAnyKey(_crypto, request.Data);
                            if (!string.IsNullOrEmpty(aesDecrypted))
                            {
                                request.Data = aesDecrypted;
                            }
                        }
                        catch { }
                    }
                }

                var response = await RouteActionAsync(request);
                response.Id = request.Id;

                // ── Encrypt outgoing data with SharedKey ──
                if (!string.IsNullOrEmpty(sharedKey) && response.Data != null)
                {
                    try
                    {
                        var plainJson = JsonSerializer.Serialize(response.Data, JsonOpts);
                        var encrypted = _crypto.Encrypt(sharedKey, plainJson);
                        response.Data = encrypted;
                    }
                    catch
                    {
                        _logger.LogWarning("WS: encrypt response failed for {Id}", connectionId);
                    }
                }

                await SendAsync(connectionId, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WS process error: {Id}", connectionId);
                await SendAsync(connectionId, new WsResponse
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        private async Task SendAsync(string connectionId, WsResponse response)
        {
            var json = JsonSerializer.Serialize(response, JsonOpts);
            await _connectionManager.SendToConnectionAsync(connectionId, json);
        }

        private async Task<WsResponse> RouteActionAsync(WsRequest request)
        {
            using var scope = _scopeFactory.CreateScope();
            var action = request.Action.ToLowerInvariant();

            try
            {
                return action switch
                {
                    // ── Auth (no session required) ──
                    "auth.handshake" => await HandleAuthHandshake(scope, request.Data),

                    // ── All other actions require valid session ──
                    _ => await RequiresSession(scope, request, action)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WS action error: {Action}", action);
                return new WsResponse { Action = action, Success = false, Message = ex.Message };
            }
        }

        private async Task<WsResponse> RequiresSession(IServiceScope scope, WsRequest request, string action)
        {
            // Validate session from request data (encrypted { clientId, sessionToken })
            var sessionService = scope.ServiceProvider.GetRequiredService<AuthService>();

            if (!string.IsNullOrEmpty(request.Auth))
            {
                var valid = await sessionService.ValidateAuthHeaderAsync(request.Auth);
                if (!valid)
                    return new WsResponse { Action = action, Success = false, Message = "Invalid or expired session" };
            }

            // ── تشخیص خودکار entity و operation ──
            var parts = action.Split('.', 2);
            if (parts.Length == 2)
            {
                var entity = parts[0];
                var operation = parts[1];
                
                // ابتدا special operations (هندلرهای اختصاصی)
                var special = await HandleSpecialOperation(scope, entity, operation, request);
                if (special != null) return special;
                
                // سپس CRUD operations خودکار
                return operation switch
                {
                    "list" or "admin" or "listadmin" => await HandleList(scope, entity),
                    "get" => await HandleGet(scope, entity, request.Data),
                    "create" => await HandleCreate(scope, entity, request.Data),
                    "update" => await HandleUpdate(scope, entity, request.Data),
                    "delete" => await HandleDelete(scope, entity, request.Data),
                    _ => new WsResponse { Action = action, Success = false, Message = $"Unknown operation: {operation}" }
                };
            }
            
            // Auth actions
            if (action == "auth.handshake")
                return await HandleAuthHandshake(scope, request.Data);
            if (action == "auth.login")
                return await HandleAuthLogin(scope, request.Data);
            
            return new WsResponse { Action = action, Success = false, Message = $"Unknown action: {action}" };
        }

        // ── Product Handlers ──────────────────────────────────
        private static async Task<WsResponse> HandleProductList(IServiceScope scope)
        {
            var svc = scope.ServiceProvider.GetRequiredService<ProductBusinessService>();
            var data = await svc.GetAllAsync();
            return new WsResponse { Action = "product.list", Success = true, Data = data };
        }

        // ── Generic CRUD Handlers (خودکار با dynamic) ──

        private static async Task<WsResponse> HandleList(IServiceScope scope, string entity)
        {
            var svc = GetService(scope, entity);
            if (svc == null)
                return new WsResponse { Action = $"{entity}.list", Success = false, Message = $"Service not found for {entity}" };

            var method = svc.GetType().GetMethod("GetAllAsync");
            if (method == null)
                return new WsResponse { Action = $"{entity}.list", Success = false, Message = $"GetAllAsync not found for {entity}" };

            dynamic result = await ((dynamic)method.Invoke(svc, null)!);
            return new WsResponse { Action = $"{entity}.list", Success = true, Data = result };
        }

        private static async Task<WsResponse> HandleGet(IServiceScope scope, string entity, string? data)
        {
            var svc = GetService(scope, entity);
            if (svc == null)
                return new WsResponse { Action = $"{entity}.get", Success = false, Message = $"Service not found for {entity}" };

            var id = int.TryParse(data, out var v) ? v : 0;
            var method = svc.GetType().GetMethod("GetByIdAsync");
            if (method == null)
                return new WsResponse { Action = $"{entity}.get", Success = false, Message = $"GetByIdAsync not found for {entity}" };

            dynamic result = await ((dynamic)method.Invoke(svc, new object[] { id })!);
            return new WsResponse { Action = $"{entity}.get", Success = result != null, Data = result };
        }

        private static async Task<WsResponse> HandleCreate(IServiceScope scope, string entity, string? data)
        {
            var svc = GetService(scope, entity);
            if (svc == null)
                return new WsResponse { Action = $"{entity}.create", Success = false, Message = $"Service not found for {entity}" };

            var method = svc.GetType().GetMethod("InsertAsync");
            if (method == null)
                return new WsResponse { Action = $"{entity}.create", Success = false, Message = $"InsertAsync not found for {entity}" };

            var dtoType = method.GetParameters()[0].ParameterType;
            var dto = JsonSerializer.Deserialize(data ?? "{}", dtoType, JsonOpts);
            dynamic result = await ((dynamic)method.Invoke(svc, new object[] { dto })!);
            return new WsResponse { Action = $"{entity}.create", Success = true, Data = new { id = (int)result } };
        }

        private static async Task<WsResponse> HandleUpdate(IServiceScope scope, string entity, string? data)
        {
            var svc = GetService(scope, entity);
            if (svc == null)
                return new WsResponse { Action = $"{entity}.update", Success = false, Message = $"Service not found for {entity}" };

            var method = svc.GetType().GetMethod("UpdateAsync");
            if (method == null)
                return new WsResponse { Action = $"{entity}.update", Success = false, Message = $"UpdateAsync not found for {entity}" };

            var dtoType = method.GetParameters()[0].ParameterType;
            var dto = JsonSerializer.Deserialize(data ?? "{}", dtoType, JsonOpts);
            dynamic result = await ((dynamic)method.Invoke(svc, new object[] { dto })!);
            return new WsResponse { Action = $"{entity}.update", Success = result };
        }

        private static async Task<WsResponse> HandleDelete(IServiceScope scope, string entity, string? data)
        {
            var svc = GetService(scope, entity);
            if (svc == null)
                return new WsResponse { Action = $"{entity}.delete", Success = false, Message = $"Service not found for {entity}" };

            var method = svc.GetType().GetMethod("DeleteAsync");
            if (method == null)
                return new WsResponse { Action = $"{entity}.delete", Success = false, Message = $"DeleteAsync not found for {entity}" };

            var id = int.TryParse(data, out var v) ? v : 0;
            dynamic result = await ((dynamic)method.Invoke(svc, new object[] { id })!);
            return new WsResponse { Action = $"{entity}.delete", Success = result, Data = result };
        }

        private static object? GetService(IServiceScope scope, string entity)
        {
            var sp = scope.ServiceProvider;
            
            return entity.ToLowerInvariant() switch
            {
                "product" => sp.GetService<ProductBusinessService>(),
                "blog" => sp.GetService<BlogBusinessService>(),
                "portfolio" => sp.GetService<PortfolioBusinessService>(),
                "contact" => sp.GetService<ContactBusinessService>(),
                "page" => sp.GetService<PageBusinessService>(),
                "role" => sp.GetService<RoleBusinessService>(),
                "permission" => sp.GetService<PermissionBusinessService>(),
                "settings" => sp.GetService<SettingsBusinessService>(),
                "client" => sp.GetService<ClientBusinessService>(),
                "event" => sp.GetService<EventBusinessService>(),
                "user" => sp.GetService<UserBusinessService>(),
                "homeslide" => sp.GetService<HomeSlideBusinessService>(),
                "homeproduct" => sp.GetService<HomeProductBusinessService>(),
                _ => null
            };
        }

        // ── Special Operations (فقط مسیرهای غیرCRUD standard) ──
        private static async Task<WsResponse?> HandleSpecialOperation(IServiceScope scope, string entity, string operation, WsRequest request)
        {
            return (entity, operation) switch
            {
                // Contact
                ("contact", "submit") => await HandleContactSubmit(scope, request.Data),
                ("contact", "markread") => await HandleContactMarkRead(scope, request.Data),
                ("contact", "unread") => await HandleContactUnread(scope),
                ("contact", "count") => await HandleContactCount(scope),

                // Product
                ("product", "category") => await HandleProductCategory(scope, request.Data),

                // Auth
                ("auth", "login") => await HandleAuthLogin(scope, request.Data),

                _ => null  // ناشناخته → به CRUD خودکار بره
            };
        }

        private static async Task<WsResponse> HandleProductGet(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<ProductBusinessService>();
            var id = int.TryParse(data, out var v) ? v : 0;
            var item = await svc.GetByIdAsync(id);
            return new WsResponse { Action = "product.get", Success = true, Data = item };
        }

        private static async Task<WsResponse> HandleProductCategory(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<ProductBusinessService>();
            var items = await svc.GetByCategoryAsync(data ?? "");
            return new WsResponse { Action = "product.category", Success = true, Data = items };
        }

        // ── Blog Handlers ─────────────────────────────────────
        private static async Task<WsResponse> HandleBlogList(IServiceScope scope)
        {
            var svc = scope.ServiceProvider.GetRequiredService<BlogBusinessService>();
            var data = await svc.GetAllAsync();
            return new WsResponse { Action = "blog.list", Success = true, Data = data };
        }

        private static async Task<WsResponse> HandleBlogGet(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<BlogBusinessService>();
            var item = await svc.GetBySlugAsync(data ?? "");
            return new WsResponse { Action = "blog.get", Success = true, Data = item };
        }

        private static async Task<WsResponse> HandleBlogAdmin(IServiceScope scope)
        {
            var svc = scope.ServiceProvider.GetRequiredService<BlogBusinessService>();
            var data = await svc.GetAllAsync();
            return new WsResponse { Action = "blog.admin", Success = true, Data = data };
        }

        // ── Portfolio Handlers ────────────────────────────────
        private static async Task<WsResponse> HandlePortfolioList(IServiceScope scope)
        {
            var svc = scope.ServiceProvider.GetRequiredService<PortfolioBusinessService>();
            var data = await svc.GetAllAsync();
            return new WsResponse { Action = "portfolio.list", Success = true, Data = data };
        }

        private static async Task<WsResponse> HandlePortfolioGet(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<PortfolioBusinessService>();
            var id = int.TryParse(data, out var v) ? v : 0;
            var item = await svc.GetByIdAsync(id);
            return new WsResponse { Action = "portfolio.get", Success = true, Data = item };
        }

        private static async Task<WsResponse> HandlePortfolioAdmin(IServiceScope scope)
        {
            var svc = scope.ServiceProvider.GetRequiredService<PortfolioBusinessService>();
            var data = await svc.GetAllAdminAsync();
            return new WsResponse { Action = "portfolio.admin", Success = true, Data = data };
        }

        // ── Contact Handlers ──────────────────────────────────
        private static async Task<WsResponse> HandleContactList(IServiceScope scope)
        {
            var svc = scope.ServiceProvider.GetRequiredService<ContactBusinessService>();
            var data = await svc.GetAllAsync();
            return new WsResponse { Action = "contact.list", Success = true, Data = data };
        }

        private static async Task<WsResponse> HandleContactUnread(IServiceScope scope)
        {
            var svc = scope.ServiceProvider.GetRequiredService<ContactBusinessService>();
            var data = await svc.GetUnreadAsync();
            return new WsResponse { Action = "contact.unread", Success = true, Data = data };
        }

        private static async Task<WsResponse> HandleContactCount(IServiceScope scope)
        {
            var svc = scope.ServiceProvider.GetRequiredService<ContactBusinessService>();
            var (total, unread) = await svc.CountAsync();
            return new WsResponse { Action = "contact.count", Success = true, Data = new { total, unread } };
        }

        private static async Task<WsResponse> HandleContactSubmit(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<ContactBusinessService>();
            var req = JsonSerializer.Deserialize<ContactRequest>(data ?? "{}", JsonOpts);
            if (req == null)
                return new WsResponse { Action = "contact.submit", Success = false, Message = "Invalid data" };
            var id = await svc.SubmitAsync(req);
            return new WsResponse { Action = "contact.submit", Success = true, Data = id };
        }

        private static async Task<WsResponse> HandleContactMarkRead(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<ContactBusinessService>();
            var id = int.TryParse(data, out var v) ? v : 0;
            var ok = await svc.MarkAsReadAsync(id);
            return new WsResponse { Action = "contact.markread", Success = ok };
        }

        private static async Task<WsResponse> HandleContactDelete(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<ContactBusinessService>();
            var id = int.TryParse(data, out var v) ? v : 0;
            var ok = await svc.DeleteAsync(id);
            return new WsResponse { Action = "contact.delete", Success = ok };
        }

        // ── Page Handlers ─────────────────────────────────────
        private static async Task<WsResponse> HandlePageList(IServiceScope scope)
        {
            var svc = scope.ServiceProvider.GetRequiredService<PageBusinessService>();
            var data = await svc.GetAllAsync();
            return new WsResponse { Action = "page.list", Success = true, Data = data };
        }

        private static async Task<WsResponse> HandlePageGet(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<PageBusinessService>();
            var item = await svc.GetBySlugAsync(data ?? "");
            return new WsResponse { Action = "page.get", Success = true, Data = item };
        }

        private static async Task<WsResponse> HandlePageById(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<PageBusinessService>();
            var id = int.TryParse(data, out var v) ? v : 0;
            var item = await svc.GetByIdAsync(id);
            return new WsResponse { Action = "page.id", Success = true, Data = item };
        }

        // ── Role Handlers ──────────────────────────────────────
        private static async Task<WsResponse> HandleRoleList(IServiceScope scope)
        {
            var svc = scope.ServiceProvider.GetRequiredService<RoleBusinessService>();
            var data = await svc.GetAllAsync();
            return new WsResponse { Action = "role.list", Success = true, Data = data };
        }

        private static async Task<WsResponse> HandleRoleGet(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<RoleBusinessService>();
            var id = int.TryParse(data, out var v) ? v : 0;
            var item = await svc.GetByIdAsync(id);
            return new WsResponse { Action = "role.get", Success = true, Data = item };
        }

        // ── Permission Handlers ────────────────────────────────
        private static async Task<WsResponse> HandlePermissionList(IServiceScope scope)
        {
            var svc = scope.ServiceProvider.GetRequiredService<PermissionBusinessService>();
            var data = await svc.GetAllAsync();
            return new WsResponse { Action = "permission.list", Success = true, Data = data };
        }

        private static async Task<WsResponse> HandlePermissionByRole(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<PermissionBusinessService>();
            var roleId = int.TryParse(data, out var v) ? v : 0;
            var items = await svc.GetByRoleIdAsync(roleId);
            return new WsResponse { Action = "permission.role", Success = true, Data = items };
        }

        // ── User Handlers ──────────────────────────────────────
        private static async Task<WsResponse> HandleUserList(IServiceScope scope)
        {
            var svc = scope.ServiceProvider.GetRequiredService<AuthBusinessService>();
            var data = await svc.GetAllUsersAsync();
            return new WsResponse { Action = "user.list", Success = true, Data = data };
        }

        private static async Task<WsResponse> HandleUserGet(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<AuthBusinessService>();
            var id = int.TryParse(data, out var v) ? v : 0;
            var user = await svc.GetUserByIdAsync(id);
            return new WsResponse { Action = "user.get", Success = true, Data = user };
        }

        private static async Task<WsResponse> HandleUserAdmin(IServiceScope scope)
        {
            var svc = scope.ServiceProvider.GetRequiredService<AuthBusinessService>();
            var data = await svc.GetAllUsersAdminAsync();
            return new WsResponse { Action = "user.admin", Success = true, Data = data };
        }

        // ── Auth Handlers ──────────────────────────────────────
        private static async Task<WsResponse> HandleAuthHandshake(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<AuthService>();
            if (string.IsNullOrEmpty(data))
                return new WsResponse { Action = "auth.handshake", Success = false, Message = "Missing encrypted payload" };

            var result = await svc.HandleHandshakeAsync(data);
            return new WsResponse
            {
                Action = "auth.handshake",
                Success = result.Success,
                Message = result.Error,
                Data = result.Success && result.EncryptedResponse != null
                    ? new { encrypted = true, data = result.EncryptedResponse }
                    : null
            };
        }

        private static async Task<WsResponse> HandleAuthLogin(IServiceScope scope, string? data)
        {
            var authSvc = scope.ServiceProvider.GetRequiredService<AuthBusinessService>();
            var jwtSvc = scope.ServiceProvider.GetRequiredService<JwtService>();
            var keyStore = scope.ServiceProvider.GetRequiredService<AesKeyStore>();

            var req = JsonSerializer.Deserialize<Pdd.ir.Business.Models.DTOs.LoginRequest>(data ?? "{}", JsonOpts);
            if (req == null)
                return new WsResponse { Action = "auth.login", Success = false, Message = "Invalid credentials" };

            var result = await authSvc.LoginAsync(req.Username, req.Password);
            if (!result.Success)
                return new WsResponse { Action = "auth.login", Success = false, Message = result.Message };

            var user = await authSvc.GetUserByUsernameAsync(req.Username);
            if (user == null)
                return new WsResponse { Action = "auth.login", Success = false, Message = "User not found" };

            var token = jwtSvc.GenerateToken(user);
            var refreshToken = Guid.NewGuid().ToString("N");
            keyStore.SetKey(req.Username, result.AesKey);

            return new WsResponse
            {
                Action = "auth.login",
                Success = true,
                Data = JsonSerializer.SerializeToElement(new LoginResponse
                {
                    Success = true,
                    Token = token,
                    RefreshToken = refreshToken,
                    AesKey = result.AesKey,
                    Username = user.Username,
                    FullName = user.FullName,
                    Role = user.Role
                })
            };
        }

        // ── Settings Handlers ──────────────────────────────────
        private static async Task<WsResponse> HandleSettingsList(IServiceScope scope)
        {
            var svc = scope.ServiceProvider.GetRequiredService<SettingsBusinessService>();
            var data = await svc.GetAllAsync();
            return new WsResponse { Action = "settings.list", Success = true, Data = data };
        }

        private static async Task<WsResponse> HandleSettingsGet(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<SettingsBusinessService>();
            var key = JsonSerializer.Deserialize<string>(data ?? "\"\"", JsonOpts);
            var value = await svc.GetAsync(key ?? "");
            return new WsResponse { Action = "settings.get", Success = true, Data = value };
        }

        private static async Task<WsResponse> HandleSettingsSet(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<SettingsBusinessService>();
            var req = JsonSerializer.Deserialize<Dictionary<string, string>>(data ?? "{}", JsonOpts);
            if (req != null)
                await svc.SetManyAsync(req);
            return new WsResponse { Action = "settings.set", Success = true };
        }

        // ── Client Handlers ──────────────────────────────────
        private static async Task<WsResponse> HandleClientList(IServiceScope scope)
        {
            var svc = scope.ServiceProvider.GetRequiredService<ClientBusinessService>();
            var data = await svc.GetAllAsync();
            return new WsResponse { Action = "client.list", Success = true, Data = data };
        }

        private static async Task<WsResponse> HandleClientListAdmin(IServiceScope scope)
        {
            var svc = scope.ServiceProvider.GetRequiredService<ClientBusinessService>();
            var data = await svc.GetAllAdminAsync();
            return new WsResponse { Action = "client.listadmin", Success = true, Data = data };
        }

        private static async Task<WsResponse> HandleClientGet(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<ClientBusinessService>();
            var id = int.TryParse(data, out var v) ? v : 0;
            var item = await svc.GetByIdAsync(id);
            return new WsResponse { Action = "client.get", Success = item != null, Data = item != null ? item : default };
        }

        private static async Task<WsResponse> HandleClientCreate(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<ClientBusinessService>();
            var req = JsonSerializer.Deserialize<ClientCreateRequest>(data ?? "{}", JsonOpts);
            var id = await svc.InsertAsync(req!);
            return new WsResponse { Action = "client.create", Success = true, Data = new { id } };
        }

        private static async Task<WsResponse> HandleClientUpdate(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<ClientBusinessService>();
            var dto = JsonSerializer.Deserialize<ClientDto>(data ?? "{}", JsonOpts);
            var success = await svc.UpdateAsync(dto!);
            return new WsResponse { Action = "client.update", Success = success };
        }

        private static async Task<WsResponse> HandleClientDelete(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<ClientBusinessService>();
            var id = int.TryParse(data, out var v) ? v : 0;
            var success = await svc.DeleteAsync(id);
            return new WsResponse { Action = "client.delete", Success = success, Data = success };
        }

        // ── Event Handlers ─────────────────────────────────────
        private static async Task<WsResponse> HandleEventList(IServiceScope scope)
        {
            var svc = scope.ServiceProvider.GetRequiredService<EventBusinessService>();
            var data = await svc.GetAllAsync();
            return new WsResponse { Action = "event.list", Success = true, Data = data };
        }

        private static async Task<WsResponse> HandleEventGet(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<EventBusinessService>();
            var id = int.TryParse(data, out var v) ? v : 0;
            var item = await svc.GetByIdAsync(id);
            return new WsResponse { Action = "event.get", Success = item != null, Data = item };
        }

        private static async Task<WsResponse> HandleEventCreate(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<EventBusinessService>();
            var req = JsonSerializer.Deserialize<EventCreateRequest>(data ?? "{}", JsonOpts);
            var id = await svc.InsertAsync(req!);
            return new WsResponse { Action = "event.create", Success = true, Data = new { id } };
        }

        private static async Task<WsResponse> HandleEventUpdate(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<EventBusinessService>();
            var dto = JsonSerializer.Deserialize<EventDto>(data ?? "{}", JsonOpts);
            var success = await svc.UpdateAsync(dto!);
            return new WsResponse { Action = "event.update", Success = success };
        }

        private static async Task<WsResponse> HandleEventDelete(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<EventBusinessService>();
            var id = int.TryParse(data, out var v) ? v : 0;
            var success = await svc.DeleteAsync(id);
            return new WsResponse { Action = "event.delete", Success = success, Data = success };
        }
    }

    // ── Models ───────────────────────────────────────────────
    public class WsRequest
    {
        public string? Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Data { get; set; }
        public string? Auth { get; set; }
    }

    public class WsResponse
    {
        public string? Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }
    }
}

