using System.Net.Http.Json;
using Microsoft.JSInterop;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Pdd.ir.Client.Services
{
    /// <summary>
    /// Client-side security service: generates handshake, manages session.
    /// Uses shared key directly for handshake encryption (no AES key dependency).
    /// </summary>
    public class SecurityService
    {
        private readonly IJSRuntime _js;
        private readonly HttpClient _http;
        private readonly ILogger<SecurityService> _logger;

        private const string SharedKey = "pdd-ir-ws-2026-secure-key";

        private string? _clientId;
        private string? _sessionToken;
        private string? _authHeader;
        private DateTime? _expiresAt;
        private bool _handshakeInProgress;

        public bool IsAuthenticated => !string.IsNullOrEmpty(_sessionToken) && _expiresAt.HasValue && _expiresAt > DateTime.UtcNow;
        public string? AuthHeader => _authHeader;
        public string ClientId => _clientId ??= GenerateClientId();

        public SecurityService(IJSRuntime js, HttpClient http, ILogger<SecurityService> logger)
        {
            _js = js;
            _http = http;
            _logger = logger;
        }

        /// <summary>
        /// Perform full handshake with server.
        /// </summary>
        public async Task<bool> HandshakeAsync()
        {
            if (_handshakeInProgress) return false;
            _handshakeInProgress = true;

            try
            {
                // ── Build payload { clientId, timestamp, nonce } ──
                var clientId = ClientId;
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var nonce = GenerateNonce();

                var payload = JsonSerializer.Serialize(new { clientId, timestamp, nonce });

                // ── Encrypt with shared key via JS (CryptoUtils) ──
                var encrypted = await _js.InvokeAsync<string>("CryptoUtils.encryptData", payload, SharedKey);

                // ── Send to server ──
                var handshakeRequest = new { encrypted };
                var handshakeResponse = await _http.PostAsJsonAsync("api/auth/handshake", handshakeRequest);

                if (!handshakeResponse.IsSuccessStatusCode)
                {
                    var errorBody = await handshakeResponse.Content.ReadAsStringAsync();
                    _logger.LogWarning("Handshake failed: {Status} - {Body}", handshakeResponse.StatusCode, errorBody);
                    return false;
                }

                // ── Parse and decrypt response ──
                var responseJson = await handshakeResponse.Content.ReadAsStringAsync();
                var doc = JsonSerializer.Deserialize<JsonElement>(responseJson);

                if (!doc.TryGetProperty("data", out var dataProp))
                    return false;

                var encryptedData = dataProp.GetString();
                if (string.IsNullOrEmpty(encryptedData))
                    return false;

                // Decrypt response with shared key
                var decrypted = await _js.InvokeAsync<string>("CryptoUtils.decryptData", encryptedData, SharedKey);
                var responseDoc = JsonSerializer.Deserialize<JsonElement>(decrypted);

                if (responseDoc.TryGetProperty("sessionToken", out var tokenProp) &&
                    responseDoc.TryGetProperty("expiresAt", out var expiresProp))
                {
                    _sessionToken = tokenProp.GetString();
                    var expiresStr = expiresProp.GetString();
                    if (DateTime.TryParse(expiresStr, out var expires))
                        _expiresAt = expires;

                    // Build encrypted auth header for subsequent requests
                    var authPayload = JsonSerializer.Serialize(new { clientId, sessionToken = _sessionToken });
                    _authHeader = await _js.InvokeAsync<string>("CryptoUtils.encryptData", authPayload, SharedKey);

                    _logger.LogInformation("Session established: ClientId={ClientId}, Expires={Expires}", clientId, _expiresAt);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Handshake error");
                return false;
            }
            finally
            {
                _handshakeInProgress = false;
            }
        }

        /// <summary>
        /// Get auth header for requests. Auto-handshake if needed.
        /// </summary>
        public async Task<string?> GetAuthHeaderAsync()
        {
            if (!IsAuthenticated)
            {
                var ok = await HandshakeAsync();
                if (!ok) return null;
            }

            return _authHeader;
        }

        /// <summary>
        /// Clear session (logout)
        /// </summary>
        public void ClearSession()
        {
            _sessionToken = null;
            _authHeader = null;
            _expiresAt = null;
        }

        /// <summary>
        /// Encrypt data with SharedKey via JS CryptoUtils
        /// </summary>
        public async Task<string?> EncryptDataAsync(string plainText)
        {
            try
            {
                return await _js.InvokeAsync<string>("CryptoUtils.encryptData", plainText, SharedKey);
            }
            catch { return null; }
        }

        /// <summary>
        /// Decrypt data with SharedKey via JS CryptoUtils
        /// </summary>
        public async Task<string?> DecryptDataAsync(string ciphertext)
        {
            try
            {
                return await _js.InvokeAsync<string>("CryptoUtils.decryptData", ciphertext, SharedKey);
            }
            catch { return null; }
        }

        private static string GenerateNonce()
        {
            var bytes = RandomNumberGenerator.GetBytes(16);
            return Convert.ToBase64String(bytes);
        }

        private static string GenerateClientId()
        {
            var machineHash = Environment.MachineName.GetHashCode().ToString("X8");
            return $"pdd-{machineHash}";
        }
    }
}
