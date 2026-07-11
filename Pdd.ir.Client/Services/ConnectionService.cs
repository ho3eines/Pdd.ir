using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Pdd.ir.Client.Services
{
    public class ConnectionService : IAsyncDisposable
    {
        private readonly HttpClient _http;
        private readonly ILogger<ConnectionService> _logger;
        private ClientWebSocket? _webSocket;
        private bool _isConnected;
        private Timer? _reconnectTimer;

        public bool IsWebSocketConnected => _isConnected;

        public event Action<bool>? OnConnectionChanged;

        public ConnectionService(HttpClient http, ILogger<ConnectionService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task ConnectAsync(string wsUrl)
        {
            try
            {
                _webSocket = new ClientWebSocket();
                await _webSocket.ConnectAsync(new Uri(wsUrl), CancellationToken.None);
                _isConnected = true;
                OnConnectionChanged?.Invoke(true);
                _logger.LogInformation("WebSocket connected");

                _ = ReceiveLoopAsync();
            }
            catch (Exception ex)
            {
                _isConnected = false;
                OnConnectionChanged?.Invoke(false);
                _logger.LogWarning("WebSocket connection failed, will use API fallback: {Error}", ex.Message);

                _reconnectTimer = new Timer(async _ => await TryReconnectAsync(wsUrl), null, 5000, 5000);
            }
        }

        private async Task TryReconnectAsync(string wsUrl)
        {
            if (_isConnected) return;

            try
            {
                _webSocket?.Dispose();
                _webSocket = new ClientWebSocket();
                await _webSocket.ConnectAsync(new Uri(wsUrl), CancellationToken.None);
                _isConnected = true;
                OnConnectionChanged?.Invoke(true);
                _reconnectTimer?.Dispose();

                _ = ReceiveLoopAsync();
            }
            catch
            {
                // Will retry on next timer tick
            }
        }

        private async Task ReceiveLoopAsync()
        {
            var buffer = new byte[1024 * 4];
            try
            {
                while (_webSocket?.State == WebSocketState.Open)
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _isConnected = false;
                        OnConnectionChanged?.Invoke(false);
                    }
                }
            }
            catch
            {
                _isConnected = false;
                OnConnectionChanged?.Invoke(false);
            }
        }

        public async Task<string?> SendViaWebSocketAsync(string action, string? data = null)
        {
            if (!_isConnected || _webSocket?.State != WebSocketState.Open)
                return null;

            try
            {
                var request = JsonSerializer.Serialize(new { action, data });
                var bytes = Encoding.UTF8.GetBytes(request);
                await _webSocket!.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);

                var buffer = new byte[1024 * 4];
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                return Encoding.UTF8.GetString(buffer, 0, result.Count);
            }
            catch
            {
                return null;
            }
        }

        public async Task<T?> SendViaApiAsync<T>(string url, object? data = null)
        {
            try
            {
                HttpResponseMessage response;

                if (data != null)
                    response = await _http.PostAsJsonAsync(url, data);
                else
                    response = await _http.GetAsync(url);

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<T>();
            }
            catch { }

            return default;
        }

        public async Task<T?> GetAsync<T>(string url)
        {
            try
            {
                var response = await _http.GetAsync(url);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<T>();
            }
            catch { }

            return default;
        }

        public async ValueTask DisposeAsync()
        {
            _reconnectTimer?.Dispose();
            if (_webSocket != null)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                _webSocket.Dispose();
            }
        }
    }
}
