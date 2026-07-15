using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Pdd.ir.Client.Services
{
    public interface ICommunicationService
    {
        bool IsWebSocketConnected { get; }
        event Action<bool>? OnConnectionChanged;
        Task InitializeAsync();
        Task<T?> GetAsync<T>(string url);
        Task<T?> PostAsync<T>(string url, object data);
        Task<T?> PutAsync<T>(string url, object data);
        Task<bool> DeleteAsync(string url);
    }

    public class CommunicationService : ICommunicationService, IAsyncDisposable
    {
        private readonly HttpClient _http;
        private ClientWebSocket? _ws;
        private bool _isConnected;
        private string _wsUrl = "";
        private Timer? _reconnectTimer;
        private int _reconnectAttempts;
        private readonly SemaphoreSlim _wsLock = new(1, 1);
        private const int MaxReconnectAttempts = 10;
        private const int InitialRetryMs = 2000;
        private const int MaxRetryMs = 30000;
        private const int WsTimeoutMs = 5000;

        public bool IsWebSocketConnected => _isConnected;
        public event Action<bool>? OnConnectionChanged;

        public CommunicationService(HttpClient http)
        {
            _http = http;
        }

        public async Task InitializeAsync()
        {
            try
            {
                var baseUri = _http.BaseAddress?.ToString().TrimEnd('/');
                if (string.IsNullOrEmpty(baseUri)) return;

                var wsUri = baseUri.Replace("http://", "ws://").Replace("https://", "wss://");
                _wsUrl = wsUri + "ws";

                await ConnectAsync();
            }
            catch { }
        }

        private async Task ConnectAsync()
        {
            try
            {
                _ws?.Dispose();
                _ws = new ClientWebSocket();
                await _ws.ConnectAsync(new Uri(_wsUrl), CancellationToken.None);
                _isConnected = true;
                _reconnectAttempts = 0;
                OnConnectionChanged?.Invoke(true);
            }
            catch
            {
                _isConnected = false;
                OnConnectionChanged?.Invoke(false);
                ScheduleReconnect();
            }
        }

        private void ScheduleReconnect()
        {
            _reconnectTimer?.Dispose();
            if (_reconnectAttempts >= MaxReconnectAttempts) return;

            var delay = Math.Min(InitialRetryMs * (int)Math.Pow(2, _reconnectAttempts), MaxRetryMs);
            _reconnectTimer = new Timer(async _ =>
            {
                _reconnectAttempts++;
                await ConnectAsync();
            }, null, delay, Timeout.Infinite);
        }

        public async Task<T?> GetAsync<T>(string url)
        {
            if (_isConnected)
            {
                var wsResult = await SendWsAsync<T>(url, "list");
                if (wsResult != null) return wsResult;
            }
            return await HttpGetAsync<T>(url);
        }

        public async Task<T?> PostAsync<T>(string url, object data)
        {
            if (_isConnected)
            {
                var wsResult = await SendWsAsync<T>(url, "list", data);
                if (wsResult != null) return wsResult;
            }
            return await HttpPostAsync<T>(url, data);
        }

        public async Task<T?> PutAsync<T>(string url, object data)
        {
            try
            {
                var response = await _http.PutAsJsonAsync(url, data);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<T>();
            }
            catch { }
            return default;
        }

        public async Task<bool> DeleteAsync(string url)
        {
            try
            {
                var response = await _http.DeleteAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        private async Task<T?> SendWsAsync<T>(string url, string action, object? data = null)
        {
            if (_ws?.State != WebSocketState.Open) return default;

            await _wsLock.WaitAsync();
            try
            {
                var wsAction = MapUrlToAction(url, action);
                var payload = data != null
                    ? JsonSerializer.Serialize(new { action = wsAction, data = JsonSerializer.Serialize(data) })
                    : JsonSerializer.Serialize(new { action = wsAction });

                var sendBytes = Encoding.UTF8.GetBytes(payload);
                await _ws.SendAsync(new ArraySegment<byte>(sendBytes), WebSocketMessageType.Text, true, CancellationToken.None);

                var receiveBytes = new byte[1024 * 64];
                using var cts = new CancellationTokenSource(WsTimeoutMs);
                var result = await _ws.ReceiveAsync(new ArraySegment<byte>(receiveBytes), cts.Token);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _isConnected = false;
                    OnConnectionChanged?.Invoke(false);
                    ScheduleReconnect();
                    return default;
                }

                var responseJson = Encoding.UTF8.GetString(receiveBytes, 0, result.Count);
                var wsResponse = JsonSerializer.Deserialize<WsClientResponse>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (wsResponse?.Success != true || wsResponse.Data == null)
                    return default;

                return wsResponse.Data.Value.Deserialize<T>(new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (OperationCanceledException) { return default; }
            catch (WebSocketException)
            {
                _isConnected = false;
                OnConnectionChanged?.Invoke(false);
                ScheduleReconnect();
                return default;
            }
            catch { return default; }
            finally { _wsLock.Release(); }
        }

        private async Task<T?> HttpGetAsync<T>(string url)
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

        private async Task<T?> HttpPostAsync<T>(string url, object data)
        {
            try
            {
                var response = await _http.PostAsJsonAsync(url, data);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<T>();
            }
            catch { }
            return default;
        }

        private static string MapUrlToAction(string url, string defaultAction)
        {
            var lower = url.ToLowerInvariant();
            if (lower.Contains("product")) return "product." + defaultAction;
            if (lower.Contains("blog")) return "blog." + defaultAction;
            if (lower.Contains("portfolio")) return "portfolio." + defaultAction;
            if (lower.Contains("contact")) return "contact." + defaultAction;
            if (lower.Contains("page")) return "page." + defaultAction;
            if (lower.Contains("user")) return "user." + defaultAction;
            if (lower.Contains("role")) return "role." + defaultAction;
            if (lower.Contains("permission")) return "permission." + defaultAction;
            if (lower.Contains("auth")) return "auth." + defaultAction;
            return "unknown." + defaultAction;
        }

        public async ValueTask DisposeAsync()
        {
            _reconnectTimer?.Dispose();
            _wsLock.Dispose();
            if (_ws != null)
            {
                try
                {
                    if (_ws.State == WebSocketState.Open)
                        await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                }
                catch { }
                _ws.Dispose();
            }
        }

        private class WsClientResponse
        {
            public string Action { get; set; } = "";
            public bool Success { get; set; }
            public string? Message { get; set; }
            public JsonElement? Data { get; set; }
        }
    }
}
