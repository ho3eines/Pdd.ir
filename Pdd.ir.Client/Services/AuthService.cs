using System.Net.Http.Json;
using System.Text.Json;

namespace Pdd.ir.Client.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly EncryptionService _encryption;

        public string? Token { get; private set; }
        public string? RefreshToken { get; private set; }
        public string? Username { get; private set; }
        public string? FullName { get; private set; }
        public string? Role { get; private set; }
        public bool IsAuthenticated => !string.IsNullOrEmpty(Token);

        public event Action? OnAuthStateChanged;

        public AuthService(HttpClient http, EncryptionService encryption)
        {
            _http = http;
            _encryption = encryption;
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
            OnAuthStateChanged?.Invoke();
        }
    }
}
