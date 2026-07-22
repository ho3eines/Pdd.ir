using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using Pdd.ir.Client.Models;

namespace Pdd.ir.Client.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly EncryptionService _encryption;
        private readonly ICommunicationService _comm;
        private readonly IJSRuntime _js;

        public string? Token { get; private set; }
        public string? RefreshToken { get; private set; }
        public string? Username { get; private set; }
        public string? FullName { get; private set; }
        public string? Role { get; private set; }
        public bool IsAuthenticated => !string.IsNullOrEmpty(Token);

        public event Action? OnAuthStateChanged;

        public AuthService(HttpClient http, EncryptionService encryption, ICommunicationService comm, IJSRuntime js)
        {
            _http = http;
            _encryption = encryption;
            _comm = comm;
            _js = js;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                // ✅ استفاده از Comm به جای HttpClient مستقیم
                // WS: برمیگردونه LoginResponse خام
                // HTTP: برمیگردونه ApiResponse<LoginResponse> (با data wrapper)
                var response = await _comm.PostAsync<JsonElement>("api/auth/login", new { Username = username, Password = password });

                if (response.ValueKind == JsonValueKind.Undefined || response.ValueKind == JsonValueKind.Null)
                    return false;

                // تشخیص فرمت: اگه "data" property داره → HTTP format، وگرنه → WS format
                JsonElement loginData;
                if (response.TryGetProperty("data", out var dataProp) && dataProp.ValueKind == JsonValueKind.Object)
                    loginData = dataProp;
                else
                    loginData = response;

                var success = loginData.TryGetProperty("success", out var s) && s.GetBoolean();
                if (!success) return false;

                Token = loginData.TryGetProperty("token", out var t) ? t.GetString() : null;
                RefreshToken = loginData.TryGetProperty("refreshToken", out var rt) ? rt.GetString() : null;
                Username = loginData.TryGetProperty("username", out var u) ? u.GetString() : null;
                FullName = loginData.TryGetProperty("fullName", out var fn) ? fn.GetString() : null;
                Role = loginData.TryGetProperty("role", out var r) ? r.GetString() : null;

                if (loginData.TryGetProperty("aesKey", out var aes) && aes.ValueKind == JsonValueKind.String)
                {
                    var aesKey = aes.GetString();
                    if (!string.IsNullOrEmpty(aesKey))
                        _encryption.SetKey(aesKey);
                }

                if (string.IsNullOrEmpty(Token)) return false;

                _http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
                _http.DefaultRequestHeaders.Remove("X-Username");
                _http.DefaultRequestHeaders.Add("X-Username", Username ?? "");

                await SaveToLocalStorageAsync();
                OnAuthStateChanged?.Invoke();

                // ── Reconnect WS with new AES key ──
                try { await _comm.ReconnectAsync(); } catch { }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Logout()
        {
            Token = null;
            RefreshToken = null;
            Username = null;
            FullName = null;
            Role = null;
            _encryption.ClearKey();
            _http.DefaultRequestHeaders.Authorization = null;
            _http.DefaultRequestHeaders.Remove("X-Username");
            _ = ClearLocalStorageAsync();
            OnAuthStateChanged?.Invoke();
        }

        public bool HasRole(string role)
        {
            return string.Equals(Role, role, StringComparison.OrdinalIgnoreCase);
        }

        public bool HasAnyRole(params string[] roles)
        {
            return roles.Any(r => HasRole(r));
        }

        public async Task LoadFromStorageAsync()
        {
            try
            {
                Token = await _js.InvokeAsync<string?>("localStorage.getItem", "auth_token");
                RefreshToken = await _js.InvokeAsync<string?>("localStorage.getItem", "auth_refreshToken");

                // ✅ اطلاعات کاربر از localStorage خوانده نمی‌شود — فقط توکن‌ها

                if (!string.IsNullOrEmpty(Token))
                {
                    _http.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
                    _http.DefaultRequestHeaders.Remove("X-Username");
                    OnAuthStateChanged?.Invoke();
                }
            }
            catch { }
        }

        private async Task SaveToLocalStorageAsync()
        {
            try
            {
                // ✅ فقط توکن‌ها ذخیره شوند (نه اطلاعات کاربر)
                await _js.InvokeVoidAsync("localStorage.setItem", "auth_token", Token);
                await _js.InvokeVoidAsync("localStorage.setItem", "auth_refreshToken", RefreshToken);
            }
            catch { }
        }

        private async Task ClearLocalStorageAsync()
        {
            try
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", "auth_token");
                await _js.InvokeVoidAsync("localStorage.removeItem", "auth_refreshToken");
            }
            catch { }
        }
    }
}
