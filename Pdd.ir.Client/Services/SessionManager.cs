using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Pdd.ir.Client.Services
{
    /// <summary>
    /// Manages session-based auth: handshake with server, stores session token.
    /// Flow: encrypt { clientId, timestamp } → send → receive encrypted { sessionToken }
    /// </summary>
    public class SessionManager
    {
        private readonly EncryptionService _encryption;
        private readonly HttpClient _http;
        private readonly ILogger<SessionManager> _logger;
        private string? _clientId;
        private string? _sessionToken;
        private string? _authHeader; // encrypted { clientId, sessionToken }

        private const string SharedKey = "pdd-ir-ws-2026-secure-key";

        public bool IsAuthenticated => !string.IsNullOrEmpty(_sessionToken);
        public string? AuthHeader => _authHeader;
        public string ClientId => _clientId ??= GetOrCreateClientId();

        public SessionManager(EncryptionService encryption, HttpClient http, ILogger<SessionManager> logger)
        {
            _encryption = encryption;
            _http = http;
            _logger = logger;
        }

        /// <summary>
        /// Perform handshake with server. Returns true if successful.
        /// </summary>
        public async Task<bool> HandshakeAsync()
        {
            try
            {
                var clientId = ClientId;
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                // Encrypt { clientId, timestamp } with shared key
                var payload = JsonSerializer.Serialize(new { clientId, timestamp });
                var encrypted = await _encryption.EncryptAsync(payload);

                // Send to server (HTTP fallback for handshake)
                var request = new { encrypted };
                var response = await _http.PostAsJsonAsync("api/auth/handshake", request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Handshake failed: {Status}", response.StatusCode);
                    return false;
                }

                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonSerializer.Deserialize<JsonElement>(json);

                if (!doc.TryGetProperty("data", out var dataProp))
                    return false;

                var encryptedData = dataProp.GetString();
                if (string.IsNullOrEmpty(encryptedData))
                    return false;

                // Decrypt response { sessionToken }
                var decrypted = await _encryption.DecryptAsync(encryptedData);
                var responseDoc = JsonSerializer.Deserialize<JsonElement>(decrypted);

                if (responseDoc.TryGetProperty("sessionToken", out var tokenProp))
                {
                    _sessionToken = tokenProp.GetString();
                    _authHeader = await _encryption.EncryptAsync(JsonSerializer.Serialize(new { clientId, sessionToken = _sessionToken }));
                    _logger.LogInformation("Handshake successful, session established");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Handshake error");
                return false;
            }
        }

        /// <summary>
        /// Get encrypted auth header for requests
        /// </summary>
        public async Task<string?> GetAuthHeaderAsync()
        {
            if (string.IsNullOrEmpty(_sessionToken))
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
        }

        private string GetOrCreateClientId()
        {
            // Generate or retrieve clientId from a stable source
            // For now, use a fixed ID based on machine fingerprint
            _clientId = "pdd-client-" + Environment.MachineName.GetHashCode().ToString("X8");
            return _clientId;
        }
    }
}
