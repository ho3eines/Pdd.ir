using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Pdd.ir.Client.Services
{
    public interface ICommunicationService
    {
        bool IsWebSocketConnected { get; }
        bool IsAuthenticated { get; }
        event Action<bool>? OnConnectionChanged;
        Task InitializeAsync();
        Task<bool> AuthenticateAsync();
        Task ReconnectAsync();
        Task<T?> GetAsync<T>(string url);
        Task<T?> PostAsync<T>(string url, object? data = null);
        Task<T?> PutAsync<T>(string url, object? data = null);
        Task<bool> DeleteAsync(string url);
    }

    public class CommunicationService : ICommunicationService, IAsyncDisposable
    {
        private readonly HttpClient _http;
        private readonly EncryptionService _encryption;
        private readonly SecurityService _security;
        private readonly ILogger<CommunicationService> _logger;
        private ClientWebSocket? _ws;
        private bool _isConnected;
        private string _wsUrl = "";
        private Timer? _reconnectTimer;
        private int _reconnectAttempts;
        private readonly SemaphoreSlim _wsLock = new(1, 1);
        private CancellationTokenSource? _receiveCts;

        private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _pendingRequests = new();

        private const int MaxReconnectAttempts = 10;
        private const int InitialRetryMs = 2000;
        private const int MaxRetryMs = 30000;
        private const int WsTimeoutMs = 10000;

        public bool IsWebSocketConnected => _isConnected;
        public bool IsAuthenticated => _security.IsAuthenticated;
        public event Action<bool>? OnConnectionChanged;

        public CommunicationService(HttpClient http, EncryptionService encryption, SecurityService security, ILogger<CommunicationService> logger)
        {
            _http = http;
            _encryption = encryption;
            _security = security;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            try
            {
                var baseUri = _http.BaseAddress?.ToString().TrimEnd('/');
                if (string.IsNullOrEmpty(baseUri)) return;

                var wsUri = baseUri.Replace("http://", "ws://").Replace("https://", "wss://");
                _wsUrl = wsUri + "/ws";

                // Auto-authenticate
                await AuthenticateAsync();
            }
            catch { }
        }

        /// <summary>
        /// Perform handshake with server to get session token
        /// </summary>
        public async Task<bool> AuthenticateAsync()
        {
            var ok = await _security.HandshakeAsync();
            if (ok)
            {
                // Add auth header to HttpClient
                _http.DefaultRequestHeaders.Remove("X-Auth");
                var authHeader = await _security.GetAuthHeaderAsync();
                if (!string.IsNullOrEmpty(authHeader))
                    _http.DefaultRequestHeaders.Add("X-Auth", authHeader);
            }
            return ok;
        }

        public async Task ReconnectAsync()
        {
            _reconnectAttempts = 0;

            if (_ws != null && _ws.State == WebSocketState.Open)
            {
                try { await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None); } catch { }
            }

            await ConnectAsync();
        }

        private async Task ConnectAsync()
        {
            try
            {
                _ws?.Dispose();
                _ws = new ClientWebSocket();

                var url = _wsUrl;
                await _ws.ConnectAsync(new Uri(url), CancellationToken.None);
                _isConnected = true;
                _reconnectAttempts = 0;
                OnConnectionChanged?.Invoke(true);

                _receiveCts?.Cancel();
                _receiveCts = new CancellationTokenSource();
                _ = ReceiveLoopAsync(_receiveCts.Token);

                // Send handshake over WS
                _ = SendHandshakeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning("WS connection failed: {Error}", ex.Message);
                _isConnected = false;
                OnConnectionChanged?.Invoke(false);
                ScheduleReconnect();
            }
        }

        private async Task SendHandshakeAsync()
        {
            if (!IsAuthenticated || _ws?.State != WebSocketState.Open) return;

            try
            {
                var clientId = _security.ClientId;
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var payload = JsonSerializer.Serialize(new { clientId, timestamp });
                var encrypted = await _encryption.EncryptAsync(payload);

                var request = new { id = Guid.NewGuid().ToString("N"), action = "auth.handshake", data = encrypted };
                var json = JsonSerializer.Serialize(request);
                var sendBytes = Encoding.UTF8.GetBytes(json);

                await _wsLock.WaitAsync();
                try
                {
                    await _ws!.SendAsync(new ArraySegment<byte>(sendBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                finally
                {
                    _wsLock.Release();
                }
            }
            catch { }
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

        private async Task ReceiveLoopAsync(CancellationToken ct)
        {
            var buffer = new byte[1024 * 64];
            try
            {
                while (_ws?.State == WebSocketState.Open && !ct.IsCancellationRequested)
                {
                    var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _isConnected = false;
                        OnConnectionChanged?.Invoke(false);
                        ScheduleReconnect();
                        return;
                    }

                    if (result.EndOfMessage && result.MessageType == WebSocketMessageType.Text)
                    {
                        var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        HandleIncomingMessage(json);
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (WebSocketException)
            {
                _isConnected = false;
                OnConnectionChanged?.Invoke(false);
                ScheduleReconnect();
            }
            catch { }
        }

        private void HandleIncomingMessage(string json)
        {
            try
            {
                var response = JsonSerializer.Deserialize<WsClientResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (response == null) return;

                // Decrypt incoming data if encrypted
                if (response.Success && response.Data.HasValue && response.Data.Value.ValueKind == JsonValueKind.String)
                {
                    var encryptedStr = response.Data.Value.GetString();
                    if (!string.IsNullOrEmpty(encryptedStr))
                    {
                        try
                        {
                            var decrypted = _encryption.DecryptAsync(encryptedStr).GetAwaiter().GetResult();
                            response.DecryptedData = decrypted;
                        }
                        catch { }
                    }
                }

                if (!string.IsNullOrEmpty(response.Id) && _pendingRequests.TryRemove(response.Id, out var tcs))
                {
                    var responseData = response.DecryptedData
                        ?? (response.Data.HasValue ? response.Data.Value.GetRawText() : "null");
                    tcs.TrySetResult(responseData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("WS handle incoming error: {Error}", ex.Message);
            }
        }

        // ── Public API Methods ──

        public async Task<T?> GetAsync<T>(string url)
        {
            if (_isConnected && _ws?.State == WebSocketState.Open)
            {
                var (action, data) = MapUrlToAction(url);
                var wsResult = await SendWsAsync<T>(action, data);
                if (wsResult != null) return wsResult;
            }
            return await HttpGetAsync<T>(url);
        }

        public async Task<T?> PostAsync<T>(string url, object? data = null)
        {
            if (_isConnected && _ws?.State == WebSocketState.Open)
            {
                var (action, _) = MapUrlToAction(url);
                var dataStr = data != null ? JsonSerializer.Serialize(data) : null;
                var wsResult = await SendWsAsync<T>(action, dataStr);
                if (wsResult != null) return wsResult;
            }
            return await HttpPostAsync<T>(url, data);
        }

        public async Task<T?> PutAsync<T>(string url, object? data = null)
        {
            if (_isConnected && _ws?.State == WebSocketState.Open)
            {
                var (action, _) = MapUrlToAction(url);
                var dataStr = data != null ? JsonSerializer.Serialize(data) : null;
                var wsResult = await SendWsAsync<T>(action, dataStr);
                if (wsResult != null) return wsResult;
            }
            return await HttpPutAsync<T>(url, data);
        }

        public async Task<bool> DeleteAsync(string url)
        {
            if (_isConnected && _ws?.State == WebSocketState.Open)
            {
                var (action, data) = MapUrlToAction(url);
                var result = await SendWsAsync<object>(action, data);
                if (result != null) return true;
            }
            return await HttpDeleteAsync(url);
        }

        // ── WS Send with Auth + Request ID Correlation ──

        private async Task<T?> SendWsAsync<T>(string action, string? data = null)
        {
            if (_ws?.State != WebSocketState.Open) return default;

            var requestId = Guid.NewGuid().ToString("N");
            var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pendingRequests[requestId] = tcs;

            try
            {
                var sendData = data;
                if (!string.IsNullOrEmpty(data) && _encryption.HasKey)
                {
                    try
                    {
                        var encrypted = await _encryption.EncryptAsync(data);
                        sendData = encrypted;
                    }
                    catch { }
                }

                var authHeader = await _security.GetAuthHeaderAsync();
                var request = new { id = requestId, action, data = sendData, auth = authHeader };
                var json = JsonSerializer.Serialize(request);
                var sendBytes = Encoding.UTF8.GetBytes(json);

                await _wsLock.WaitAsync();
                try
                {
                    await _ws.SendAsync(new ArraySegment<byte>(sendBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                finally
                {
                    _wsLock.Release();
                }

                using var cts = new CancellationTokenSource(WsTimeoutMs);
                using var reg = cts.Token.Register(() => tcs.TrySetCanceled());

                var responseJson = await tcs.Task;
                if (string.IsNullOrEmpty(responseJson) || responseJson == "null") return default;

                return JsonSerializer.Deserialize<T>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (OperationCanceledException)
            {
                _pendingRequests.TryRemove(requestId, out _);
                return default;
            }
            catch (WebSocketException)
            {
                _pendingRequests.TryRemove(requestId, out _);
                _isConnected = false;
                OnConnectionChanged?.Invoke(false);
                ScheduleReconnect();
                return default;
            }
            catch
            {
                _pendingRequests.TryRemove(requestId, out _);
                return default;
            }
        }

        // ── URL to WS Action mapping ──

        private static (string action, string? data) MapUrlToAction(string url)
        {
            var lower = url.ToLowerInvariant().TrimStart('/');

            if (lower.Contains("contact") && lower.Contains("markread"))
                return ("contact.markread", ExtractId(lower));
            if (lower.Contains("contact") && lower.Contains("unread"))
                return ("contact.unread", null);
            if (lower.Contains("contact") && lower.Contains("count"))
                return ("contact.count", null);
            if (lower.Contains("role") && lower.Contains("permission"))
                return ("permission.role", ExtractId(lower));
            if (lower.Contains("blog") && lower.Contains("admin"))
                return ("blog.admin", null);
            if (lower.Contains("portfolio") && lower.Contains("admin"))
                return ("portfolio.admin", null);
            if (lower.Contains("user") && lower.Contains("admin"))
                return ("user.admin", null);
            if (lower.Contains("user") && lower.Contains("/password"))
                return ("user.password", ExtractId(lower));

            if (lower.Contains("product"))
            { var id = ExtractId(lower); return id != null ? ("product.get", id) : ("product.list", null); }
            if (lower.Contains("blog"))
            { var id = ExtractId(lower); return id != null ? ("blog.get", id) : ("blog.list", null); }
            if (lower.Contains("portfolio"))
            { var id = ExtractId(lower); return id != null ? ("portfolio.get", id) : ("portfolio.list", null); }
            if (lower.Contains("contact"))
            { var id = ExtractId(lower); return id != null ? ("contact.delete", id) : ("contact.list", null); }
            if (lower.Contains("page"))
            { var id = ExtractId(lower); return id != null ? ("page.get", id) : ("page.list", null); }
            if (lower.Contains("user"))
            { var id = ExtractId(lower); return id != null ? ("user.get", id) : ("user.list", null); }
            if (lower.Contains("role"))
            { var id = ExtractId(lower); return id != null ? ("role.get", id) : ("role.list", null); }
            if (lower.Contains("permission"))
                return ("permission.list", null);
            if (lower.Contains("settings"))
                return ("settings.list", null);
            if (lower.Contains("auth") && lower.Contains("login"))
                return ("auth.login", null);

            return ("unknown.list", null);
        }

        private static string? ExtractId(string url)
        {
            var parts = url.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return null;
            var last = parts[^1];
            if (last is "api" or "admin" or "unread" or "count" or "markread" or "password") return null;
            return last;
        }

        // ── HTTP Fallback ──

        private async Task<T?> HttpGetAsync<T>(string url)
        {
            try
            {
                var response = await _http.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return DecryptHttpResponse<T>(json);
                }
            }
            catch { }
            return default;
        }

        private async Task<T?> HttpPostAsync<T>(string url, object? data)
        {
            try
            {
                var response = data != null
                    ? await _http.PostAsJsonAsync(url, data)
                    : await _http.PostAsync(url, null);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return DecryptHttpResponse<T>(json);
                }
            }
            catch { }
            return default;
        }

        private async Task<T?> HttpPutAsync<T>(string url, object? data)
        {
            try
            {
                var response = data != null
                    ? await _http.PutAsJsonAsync(url, data)
                    : await _http.PutAsync(url, null);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return DecryptHttpResponse<T>(json);
                }
            }
            catch { }
            return default;
        }

        private async Task<bool> HttpDeleteAsync(string url)
        {
            try
            {
                var response = await _http.DeleteAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        private T? DecryptHttpResponse<T>(string json)
        {
            try
            {
                var doc = JsonSerializer.Deserialize<JsonElement>(json);
                if (doc.TryGetProperty("encrypted", out var isEncrypted) && isEncrypted.GetBoolean())
                {
                    var encryptedData = doc.GetProperty("data").GetString();
                    if (!string.IsNullOrEmpty(encryptedData) && _encryption.HasKey)
                    {
                        var decrypted = _encryption.DecryptAsync(encryptedData).GetAwaiter().GetResult();
                        return JsonSerializer.Deserialize<T>(decrypted, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                    }
                }
            }
            catch { }

            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async ValueTask DisposeAsync()
        {
            _reconnectTimer?.Dispose();
            _receiveCts?.Cancel();
            _receiveCts?.Dispose();
            _wsLock.Dispose();

            foreach (var kvp in _pendingRequests)
                kvp.Value.TrySetCanceled();
            _pendingRequests.Clear();

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
            public string? Id { get; set; }
            public string Action { get; set; } = "";
            public bool Success { get; set; }
            public string? Message { get; set; }
            public JsonElement? Data { get; set; }

            [System.Text.Json.Serialization.JsonIgnore]
            public string? DecryptedData { get; set; }
        }
    }
}
