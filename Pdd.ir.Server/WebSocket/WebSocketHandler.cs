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

                // ── Decrypt incoming data if encrypted ──
                var username = _connectionManager.GetUsername(connectionId);
                if (!string.IsNullOrEmpty(request.Data) && !string.IsNullOrEmpty(username))
                {
                    var aesKey = _keyStore.GetKey(username);
                    if (!string.IsNullOrEmpty(aesKey))
                    {
                        try
                        {
                            request.Data = _crypto.Decrypt(aesKey, request.Data);
                        }
                        catch
                        {
                            // Not encrypted or invalid — use as-is
                            _logger.LogDebug("WS: data not encrypted or decrypt failed for {Id}", connectionId);
                        }
                    }
                }

                var response = await RouteActionAsync(request);
                response.Id = request.Id;

                // ── Encrypt outgoing data ──
                if (!string.IsNullOrEmpty(username))
                {
                    var aesKey = _keyStore.GetKey(username);
                    if (!string.IsNullOrEmpty(aesKey) && response.Data.HasValue)
                    {
                        try
                        {
                            var plainJson = response.Data.Value.GetRawText();
                            var encrypted = _crypto.Encrypt(aesKey, plainJson);
                            response.Data = JsonSerializer.SerializeToElement(encrypted);
                        }
                        catch
                        {
                            _logger.LogWarning("WS: encrypt response failed for {Id}", connectionId);
                        }
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

            return action switch
            {
                // ── Product ──
                "product.list" => await HandleProductList(scope),
                "product.get" => await HandleProductGet(scope, request.Data),
                "product.category" => await HandleProductCategory(scope, request.Data),

                // ── Blog ──
                "blog.list" => await HandleBlogList(scope),
                "blog.get" => await HandleBlogGet(scope, request.Data),
                "blog.admin" => await HandleBlogAdmin(scope),

                // ── Portfolio ──
                "portfolio.list" => await HandlePortfolioList(scope),
                "portfolio.get" => await HandlePortfolioGet(scope, request.Data),
                "portfolio.admin" => await HandlePortfolioAdmin(scope),

                // ── Contact ──
                "contact.list" => await HandleContactList(scope),
                "contact.unread" => await HandleContactUnread(scope),
                "contact.count" => await HandleContactCount(scope),
                "contact.submit" => await HandleContactSubmit(scope, request.Data),
                "contact.markread" => await HandleContactMarkRead(scope, request.Data),
                "contact.delete" => await HandleContactDelete(scope, request.Data),

                // ── Page ──
                "page.list" => await HandlePageList(scope),
                "page.get" => await HandlePageGet(scope, request.Data),
                "page.id" => await HandlePageById(scope, request.Data),

                // ── User ──
                "user.list" => await HandleUserList(scope),
                "user.get" => await HandleUserGet(scope, request.Data),
                "user.admin" => await HandleUserAdmin(scope),

                // ── Role ──
                "role.list" => await HandleRoleList(scope),
                "role.get" => await HandleRoleGet(scope, request.Data),

                // ── Permission ──
                "permission.list" => await HandlePermissionList(scope),
                "permission.role" => await HandlePermissionByRole(scope, request.Data),

                // ── Auth ──
                "auth.login" => await HandleAuthLogin(scope, request.Data),

                // ── Ping ──
                "ping" => new WsResponse { Action = "ping", Success = true, Data = JsonSerializer.SerializeToElement("pong") },

                _ => new WsResponse { Action = action, Success = false, Message = $"Unknown action: {action}" }
            };
        }

        // ── Product Handlers ──────────────────────────────────
        private static async Task<WsResponse> HandleProductList(IServiceScope scope)
        {
            var svc = scope.ServiceProvider.GetRequiredService<ProductBusinessService>();
            var data = await svc.GetAllAsync();
            return new WsResponse { Action = "product.list", Success = true, Data = JsonSerializer.SerializeToElement(data) };
        }

        private static async Task<WsResponse> HandleProductGet(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<ProductBusinessService>();
            var id = int.TryParse(data, out var v) ? v : 0;
            var item = await svc.GetByIdAsync(id);
            return new WsResponse { Action = "product.get", Success = true, Data = JsonSerializer.SerializeToElement(item) };
        }

        private static async Task<WsResponse> HandleProductCategory(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<ProductBusinessService>();
            var items = await svc.GetByCategoryAsync(data ?? "");
            return new WsResponse { Action = "product.category", Success = true, Data = JsonSerializer.SerializeToElement(items) };
        }

        // ── Blog Handlers ─────────────────────────────────────
        private static async Task<WsResponse> HandleBlogList(IServiceScope scope)
        {
            var svc = scope.ServiceProvider.GetRequiredService<BlogBusinessService>();
            var data = await svc.GetAllAsync();
            return new WsResponse { Action = "blog.list", Success = true, Data = JsonSerializer.SerializeToElement(data) };
        }

        private static async Task<WsResponse> HandleBlogGet(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<BlogBusinessService>();
            var item = await svc.GetBySlugAsync(data ?? "");
            return new WsResponse { Action = "blog.get", Success = true, Data = JsonSerializer.SerializeToElement(item) };
        }

        private static async Task<WsResponse> HandleBlogAdmin(IServiceScope scope)
        {
            var svc = scope.ServiceProvider.GetRequiredService<BlogBusinessService>();
            var data = await svc.GetAllAsync();
            return new WsResponse { Action = "blog.admin", Success = true, Data = JsonSerializer.SerializeToElement(data) };
        }

        // ── Portfolio Handlers ────────────────────────────────
        private static async Task<WsResponse> HandlePortfolioList(IServiceScope scope)
        {
            var svc = scope.ServiceProvider.GetRequiredService<PortfolioBusinessService>();
            var data = await svc.GetAllAsync();
            return new WsResponse { Action = "portfolio.list", Success = true, Data = JsonSerializer.SerializeToElement(data) };
        }

        private static async Task<WsResponse> HandlePortfolioGet(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<PortfolioBusinessService>();
            var id = int.TryParse(data, out var v) ? v : 0;
            var item = await svc.GetByIdAsync(id);
            return new WsResponse { Action = "portfolio.get", Success = true, Data = JsonSerializer.SerializeToElement(item) };
        }

        private static async Task<WsResponse> HandlePortfolioAdmin(IServiceScope scope)
        {
            var svc = scope.ServiceProvider.GetRequiredService<PortfolioBusinessService>();
            var data = await svc.GetAllAdminAsync();
            return new WsResponse { Action = "portfolio.admin", Success = true, Data = JsonSerializer.SerializeToElement(data) };
        }

        // ── Contact Handlers ──────────────────────────────────
        private static async Task<WsResponse> HandleContactList(IServiceScope scope)
        {
            var svc = scope.ServiceProvider.GetRequiredService<ContactBusinessService>();
            var data = await svc.GetAllAsync();
            return new WsResponse { Action = "contact.list", Success = true, Data = JsonSerializer.SerializeToElement(data) };
        }

        private static async Task<WsResponse> HandleContactUnread(IServiceScope scope)
        {
            var svc = scope.ServiceProvider.GetRequiredService<ContactBusinessService>();
            var data = await svc.GetUnreadAsync();
            return new WsResponse { Action = "contact.unread", Success = true, Data = JsonSerializer.SerializeToElement(data) };
        }

        private static async Task<WsResponse> HandleContactCount(IServiceScope scope)
        {
            var svc = scope.ServiceProvider.GetRequiredService<ContactBusinessService>();
            var (total, unread) = await svc.CountAsync();
            return new WsResponse { Action = "contact.count", Success = true, Data = JsonSerializer.SerializeToElement(new { total, unread }) };
        }

        private static async Task<WsResponse> HandleContactSubmit(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<ContactBusinessService>();
            var req = JsonSerializer.Deserialize<ContactRequest>(data ?? "{}", JsonOpts);
            if (req == null)
                return new WsResponse { Action = "contact.submit", Success = false, Message = "Invalid data" };
            var id = await svc.SubmitAsync(req);
            return new WsResponse { Action = "contact.submit", Success = true, Data = JsonSerializer.SerializeToElement(id) };
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
            return new WsResponse { Action = "page.list", Success = true, Data = JsonSerializer.SerializeToElement(data) };
        }

        private static async Task<WsResponse> HandlePageGet(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<PageBusinessService>();
            var item = await svc.GetBySlugAsync(data ?? "");
            return new WsResponse { Action = "page.get", Success = true, Data = JsonSerializer.SerializeToElement(item) };
        }

        private static async Task<WsResponse> HandlePageById(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<PageBusinessService>();
            var id = int.TryParse(data, out var v) ? v : 0;
            var item = await svc.GetByIdAsync(id);
            return new WsResponse { Action = "page.id", Success = true, Data = JsonSerializer.SerializeToElement(item) };
        }

        // ── Role Handlers ──────────────────────────────────────
        private static async Task<WsResponse> HandleRoleList(IServiceScope scope)
        {
            var svc = scope.ServiceProvider.GetRequiredService<RoleBusinessService>();
            var data = await svc.GetAllAsync();
            return new WsResponse { Action = "role.list", Success = true, Data = JsonSerializer.SerializeToElement(data) };
        }

        private static async Task<WsResponse> HandleRoleGet(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<RoleBusinessService>();
            var id = int.TryParse(data, out var v) ? v : 0;
            var item = await svc.GetByIdAsync(id);
            return new WsResponse { Action = "role.get", Success = true, Data = JsonSerializer.SerializeToElement(item) };
        }

        // ── Permission Handlers ────────────────────────────────
        private static async Task<WsResponse> HandlePermissionList(IServiceScope scope)
        {
            var svc = scope.ServiceProvider.GetRequiredService<PermissionBusinessService>();
            var data = await svc.GetAllAsync();
            return new WsResponse { Action = "permission.list", Success = true, Data = JsonSerializer.SerializeToElement(data) };
        }

        private static async Task<WsResponse> HandlePermissionByRole(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<PermissionBusinessService>();
            var roleId = int.TryParse(data, out var v) ? v : 0;
            var items = await svc.GetByRoleIdAsync(roleId);
            return new WsResponse { Action = "permission.role", Success = true, Data = JsonSerializer.SerializeToElement(items) };
        }

        // ── User Handlers ──────────────────────────────────────
        private static async Task<WsResponse> HandleUserList(IServiceScope scope)
        {
            var svc = scope.ServiceProvider.GetRequiredService<AuthBusinessService>();
            var data = await svc.GetAllUsersAsync();
            return new WsResponse { Action = "user.list", Success = true, Data = JsonSerializer.SerializeToElement(data) };
        }

        private static async Task<WsResponse> HandleUserGet(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<AuthBusinessService>();
            var id = int.TryParse(data, out var v) ? v : 0;
            var user = await svc.GetUserByIdAsync(id);
            return new WsResponse { Action = "user.get", Success = true, Data = JsonSerializer.SerializeToElement(user) };
        }

        private static async Task<WsResponse> HandleUserAdmin(IServiceScope scope)
        {
            var svc = scope.ServiceProvider.GetRequiredService<AuthBusinessService>();
            var data = await svc.GetAllUsersAdminAsync();
            return new WsResponse { Action = "user.admin", Success = true, Data = JsonSerializer.SerializeToElement(data) };
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
                    ? JsonSerializer.SerializeToElement(new { encrypted = true, data = result.EncryptedResponse })
                    : null
            };
        }

        private static async Task<WsResponse> HandleAuthLogin(IServiceScope scope, string? data)
        {
            var svc = scope.ServiceProvider.GetRequiredService<AuthBusinessService>();
            var req = JsonSerializer.Deserialize<Pdd.ir.Business.Models.DTOs.LoginRequest>(data ?? "{}", JsonOpts);
            if (req == null)
                return new WsResponse { Action = "auth.login", Success = false, Message = "Invalid credentials" };
            var result = await svc.LoginAsync(req.Username, req.Password);
            return new WsResponse { Action = "auth.login", Success = result.Success, Data = JsonSerializer.SerializeToElement(result) };
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
        public System.Text.Json.JsonElement? Data { get; set; }
    }
}
