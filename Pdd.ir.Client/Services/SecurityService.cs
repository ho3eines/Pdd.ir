using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Pdd.ir.Client.Services
{
    /// <summary>
    /// Client-side security service: generates handshake, manages session, handles encryption.
    /// Flow: encrypt { clientId, timestamp, nonce } → send → receive encrypted { sessionToken }
    /// </summary>
    public class SecurityService
    {
        private readonly EncryptionService _encryption;
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

        public SecurityService(EncryptionService encryption, HttpClient http, ILogger<SecurityService> logger)
        {
            _encryption = encryption;
            _http = http;
            _logger = logger;
        }

        /// <summary>
        /// Perform full handshake with server.
        /// Returns true if session established successfully.
        /// </summary>
        public async Task<bool> HandshakeAsync()
        {
            if (_handshakeInProgress) return false;
            _handshakeInProgress = true;

            try
            {
                // ── Step 1: Check clock sync ──
                var clientTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var response = await _http.PostAsJsonAsync("api/auth/handshake", new { test = true });

                // ── Step 2: Build payload { clientId, timestamp, nonce } ──
                var clientId = ClientId;
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var nonce = GenerateNonce();

                var payload = JsonSerializer.Serialize(new { clientId, timestamp, nonce });

                // ── Step 3: Encrypt with shared key ──
                var encrypted = await _encryption.EncryptAsync(payload);

                // ── Step 4: Send to server ──
                var handshakeRequest = new { encrypted };
                var handshakeResponse = await _http.PostAsJsonAsync("api/auth/handshake", handshakeRequest);

                if (!handshakeResponse.IsSuccessStatusCode)
                {
                    var errorBody = await handshakeResponse.Content.ReadAsStringAsync();
                    _logger.LogWarning("Handshake failed: {Status} - {Body}", handshakeResponse.StatusCode, errorBody);
                    return false;
                }

                // ── Step 5: Parse and decrypt response ──
                var responseJson = await handshakeResponse.Content.ReadAsStringAsync();
                var doc = JsonSerializer.Deserialize<JsonElement>(responseJson);

                if (!doc.TryGetProperty("data", out var dataProp))
                    return false;

                var encryptedData = dataProp.GetString();
                if (string.IsNullOrEmpty(encryptedData))
                    return false;

                var decrypted = await _encryption.DecryptAsync(encryptedData);
                var responseDoc = JsonSerializer.Deserialize<JsonElement>(decrypted);

                if (responseDoc.TryGetProperty("sessionToken", out var tokenProp) &&
                    responseDoc.TryGetProperty("expiresAt", out var expiresProp))
                {
                    _sessionToken = tokenProp.GetString();
                    var expiresStr = expiresProp.GetString();
                    if (DateTime.TryParse(expiresStr, out var expires))
                        _expiresAt = expires;

                    // Build encrypted auth header for subsequent requests
                    _authHeader = await _encryption.EncryptAsync(JsonSerializer.Serialize(new { clientId, sessionToken = _sessionToken }));

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
