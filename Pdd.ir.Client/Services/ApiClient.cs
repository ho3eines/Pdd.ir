using System.Net.Http.Json;
using System.Text.Json;

namespace Pdd.ir.Client.Services
{
    public class ApiClient
    {
        private readonly HttpClient _http;
        private readonly ConnectionService _connection;
        private readonly ILogger<ApiClient> _logger;

        public ApiClient(HttpClient http, ConnectionService connection, ILogger<ApiClient> logger)
        {
            _http = http;
            _connection = connection;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string url)
        {
            if (_connection.IsWebSocketConnected)
            {
                var wsResult = await GetViaWebSocketAsync<T>(url);
                if (wsResult != null) return wsResult;
            }
            return await GetViaHttpAsync<T>(url);
        }

        public async Task<T?> PostAsync<T>(string url, object data)
        {
            if (_connection.IsWebSocketConnected)
            {
                var wsResult = await PostViaWebSocketAsync<T>(url, data);
                if (wsResult != null) return wsResult;
            }
            return await PostViaHttpAsync<T>(url, data);
        }

        public async Task<T?> PutAsync<T>(string url, object data)
        {
            return await PutViaHttpAsync<T>(url, data);
        }

        public async Task<bool> DeleteAsync(string url)
        {
            try
            {
                var response = await _http.DeleteAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Delete failed: {Error}", ex.Message);
                return false;
            }
        }

        private async Task<T?> GetViaHttpAsync<T>(string url)
        {
            try
            {
                var response = await _http.GetAsync(url);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<T>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning("HTTP GET failed: {Error}", ex.Message);
            }
            return default;
        }

        private async Task<T?> PostViaHttpAsync<T>(string url, object data)
        {
            try
            {
                var response = await _http.PostAsJsonAsync(url, data);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<T>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning("HTTP POST failed: {Error}", ex.Message);
            }
            return default;
        }

        private async Task<T?> PutViaHttpAsync<T>(string url, object data)
        {
            try
            {
                var response = await _http.PutAsJsonAsync(url, data);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<T>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning("HTTP PUT failed: {Error}", ex.Message);
            }
            return default;
        }

        private async Task<T?> GetViaWebSocketAsync<T>(string url)
        {
            try
            {
                var action = UrlToWsAction(url, "list");
                var result = await _connection.SendViaWebSocketAsync(action);
                if (result != null)
                    return JsonSerializer.Deserialize<T>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                _logger.LogWarning("WS GET failed: {Error}", ex.Message);
            }
            return default;
        }

        private async Task<T?> PostViaWebSocketAsync<T>(string url, object data)
        {
            try
            {
                var action = UrlToWsAction(url, "list");
                var jsonData = JsonSerializer.Serialize(data);
                var result = await _connection.SendViaWebSocketAsync(action, jsonData);
                if (result != null)
                    return JsonSerializer.Deserialize<T>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                _logger.LogWarning("WS POST failed: {Error}", ex.Message);
            }
            return default;
        }

        private static string UrlToWsAction(string url, string defaultAction)
        {
            if (url.Contains("product")) return "product." + defaultAction;
            if (url.Contains("blog")) return "blog." + defaultAction;
            if (url.Contains("portfolio")) return "portfolio." + defaultAction;
            return "unknown." + defaultAction;
        }
    }
}
