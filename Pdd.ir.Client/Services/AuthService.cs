using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;

namespace Pdd.ir.Client.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly EncryptionService _encryption;
        private readonly IJSRuntime _js;

        public string? Token { get; private set; }
        public string? RefreshToken { get; private set; }
        public string? Username { get; private set; }
        public string? FullName { get; private set; }
        public string? Role { get; private set; }
        public bool IsAuthenticated => !string.IsNullOrEmpty(Token);

        public event Action? OnAuthStateChanged;

        public AuthService(HttpClient http, EncryptionService encryption, IJSRuntime js)
        {
            _http = http;
            _encryption = encryption;
            _js = js;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/login", new { Username = username, Password = password });

                if (!response.IsSuccessStatusCode)
                    return false;

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(json);

                var data = result.GetProperty("data");

                Token = data.GetProperty("token").GetString();
                RefreshToken = data.GetProperty("refreshToken").GetString();
                Username = data.GetProperty("username").GetString();
                FullName = data.GetProperty("fullName").GetString();
                Role = data.GetProperty("role").GetString();

                var aesKey = data.GetProperty("aesKey").GetString();
                if (!string.IsNullOrEmpty(aesKey))
                    _encryption.SetKey(aesKey);

                _http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

                await SaveToLocalStorageAsync();
                OnAuthStateChanged?.Invoke();
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
                Username = await _js.InvokeAsync<string?>("localStorage.getItem", "auth_username");
                FullName = await _js.InvokeAsync<string?>("localStorage.getItem", "auth_fullName");
                Role = await _js.InvokeAsync<string?>("localStorage.getItem", "auth_role");

                if (!string.IsNullOrEmpty(Token))
                {
                    _http.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
                    OnAuthStateChanged?.Invoke();
                }
            }
            catch { }
        }

        private async Task SaveToLocalStorageAsync()
        {
            try
            {
                await _js.InvokeVoidAsync("localStorage.setItem", "auth_token", Token);
                await _js.InvokeVoidAsync("localStorage.setItem", "auth_refreshToken", RefreshToken);
                await _js.InvokeVoidAsync("localStorage.setItem", "auth_username", Username);
                await _js.InvokeVoidAsync("localStorage.setItem", "auth_fullName", FullName);
                await _js.InvokeVoidAsync("localStorage.setItem", "auth_role", Role);
            }
            catch { }
        }

        private async Task ClearLocalStorageAsync()
        {
            try
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", "auth_token");
                await _js.InvokeVoidAsync("localStorage.removeItem", "auth_refreshToken");
                await _js.InvokeVoidAsync("localStorage.removeItem", "auth_username");
                await _js.InvokeVoidAsync("localStorage.removeItem", "auth_fullName");
                await _js.InvokeVoidAsync("localStorage.removeItem", "auth_role");
            }
            catch { }
        }
    }
}
