using Pdd.ir.Business.Services;
using Pdd.ir.Server.Services;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Pdd.ir.Server.WebSocket
{
    public class WebSocketHandler
    {
        private readonly ConnectionManager _connectionManager;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<WebSocketHandler> _logger;

        public WebSocketHandler(
            ConnectionManager connectionManager,
            IServiceScopeFactory scopeFactory,
            ILogger<WebSocketHandler> logger)
        {
            _connectionManager = connectionManager;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task HandleAsync(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400;
                return;
            }

            var connectionId = Guid.NewGuid().ToString();
            var socket = await context.WebSockets.AcceptWebSocketAsync();
            _connectionManager.AddConnection(connectionId, socket);

            _logger.LogInformation("WebSocket connected: {ConnectionId}", connectionId);

            try
            {
                var buffer = new byte[1024 * 4];
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                while (!result.CloseStatus.HasValue)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await ProcessMessageAsync(connectionId, message);
                    result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }

                await socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WebSocket error: {ConnectionId}", connectionId);
            }
            finally
            {
                _connectionManager.RemoveConnection(connectionId);
                _logger.LogInformation("WebSocket disconnected: {ConnectionId}", connectionId);
            }
        }

        private async Task ProcessMessageAsync(string connectionId, string rawMessage)
        {
            try
            {
                var request = JsonSerializer.Deserialize<WsRequest>(rawMessage, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (request == null || string.IsNullOrEmpty(request.Action))
                {
                    await SendErrorAsync(connectionId, "Invalid request");
                    return;
                }

                var response = await RouteActionAsync(request);

                var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                await _connectionManager.SendToConnectionAsync(connectionId, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing WS message");
                await SendErrorAsync(connectionId, "Internal error");
            }
        }

        private async Task<WsResponse> RouteActionAsync(WsRequest request)
        {
            using var scope = _scopeFactory.CreateScope();
            var productService = scope.ServiceProvider.GetRequiredService<ProductBusinessService>();

            return request.Action switch
            {
                "product.list" => new WsResponse
                {
                    Action = "product.list",
                    Success = true,
                    Data = JsonSerializer.SerializeToElement(await productService.GetAllAsync())
                },
                "product.get" => new WsResponse
                {
                    Action = "product.get",
                    Success = true,
                    Data = JsonSerializer.SerializeToElement(
                        await productService.GetByIdAsync(int.Parse(request.Data ?? "0")))
                },
                _ => new WsResponse { Action = request.Action, Success = false, Message = "Unknown action" }
            };
        }

        private async Task SendErrorAsync(string connectionId, string message)
        {
            var response = new WsResponse { Success = false, Message = message };
            var json = JsonSerializer.Serialize(response);
            await _connectionManager.SendToConnectionAsync(connectionId, json);
        }
    }

    public class WsRequest
    {
        public string Action { get; set; } = string.Empty;
        public string? Data { get; set; }
    }

    public class WsResponse
    {
        public string Action { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? Message { get; set; }
        public System.Text.Json.JsonElement? Data { get; set; }
    }
}
