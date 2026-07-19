using Dapper;
using Microsoft.Data.SqlClient;
using Pdd.ir.Data;
using System.Data;

namespace Pdd.ir.Server.Services
{
    public class ClientSessionService
    {
        private readonly IDbService _db;
        private readonly CryptoJsService _crypto;
        private readonly IConfiguration _config;
        private readonly ILogger<ClientSessionService> _logger;

        public ClientSessionService(IDbService db, CryptoJsService crypto, IConfiguration config, ILogger<ClientSessionService> logger)
        {
            _db = db;
            _crypto = crypto;
            _config = config;
            _logger = logger;
        }

        private string SharedKey => _config["ApiKey"] ?? "";

        /// <summary>
        /// Validate handshake: decrypt { clientId, timestamp }, check timestamp ≤ 60s, create session, return encrypted sessionToken
        /// </summary>
        public async Task<(bool Success, string? EncryptedResponse, string? Error)> HandleHandshakeAsync(string encryptedPayload)
        {
            try
            {
                // 1. Decrypt with shared key
                string json;
                try
                {
                    json = _crypto.Decrypt(SharedKey, encryptedPayload);
                }
                catch
                {
                    return (false, null, "Decryption failed");
                }

                // 2. Parse { clientId, timestamp }
                var handshake = System.Text.Json.JsonSerializer.Deserialize<HandshakePayload>(json, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (handshake == null || string.IsNullOrEmpty(handshake.ClientId) || handshake.Timestamp == 0)
                    return (false, null, "Invalid handshake payload");

                // 3. Validate timestamp ≤ 60 seconds
                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var diff = Math.Abs(now - handshake.Timestamp);
                if (diff > 60)
                {
                    _logger.LogWarning("Handshake expired: clientId={ClientId}, diff={Diff}s", handshake.ClientId, diff);
                    return (false, null, "Handshake expired (>60s)");
                }

                // 4. Generate session token
                var sessionToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
                var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30).DateTime;

                // 5. Deactivate old sessions for this clientId
                using (var conn = _db.GetConnection())
                {
                    conn.Open();
                    await conn.ExecuteAsync(
                        "UPDATE ClientSessions SET IsActive = 0 WHERE ClientId = @ClientId AND IsActive = 1",
                        new { ClientId = handshake.ClientId });
                }

                // 6. Insert new session
                using (var conn = _db.GetConnection())
                {
                    conn.Open();
                    await conn.ExecuteAsync(
                        "INSERT INTO ClientSessions (ClientId, SessionToken, CreatedAt, ExpiresAt, IsActive) VALUES (@ClientId, @SessionToken, GETUTCDATE(), @ExpiresAt, 1)",
                        new { ClientId = handshake.ClientId, SessionToken = sessionToken, ExpiresAt = expiresAt });
                }

                _logger.LogInformation("Session created: clientId={ClientId}, expires={Expires}", handshake.ClientId, expiresAt);

                // 7. Encrypt response { sessionToken }
                var responseJson = System.Text.Json.JsonSerializer.Serialize(new { sessionToken });
                var encryptedResponse = _crypto.Encrypt(SharedKey, responseJson);

                return (true, encryptedResponse, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Handshake error");
                return (false, null, "Internal error");
            }
        }

        /// <summary>
        /// Validate session: decrypt { clientId, sessionToken }, check DB
        /// </summary>
        public async Task<bool> ValidateSessionAsync(string clientId, string sessionToken)
        {
            try
            {
                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(sessionToken))
                    return false;

                using var conn = _db.GetConnection();
                conn.Open();

                var session = await conn.QueryFirstOrDefaultAsync<ClientSession>(
                    "SELECT * FROM ClientSessions WHERE ClientId = @ClientId AND SessionToken = @SessionToken AND IsActive = 1 AND ExpiresAt > GETUTCDATE()",
                    new { ClientId = clientId, SessionToken = sessionToken });

                return session != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Session validation error");
                return false;
            }
        }

        /// <summary>
        /// Decrypt and validate auth header: "encrypted { clientId, sessionToken }"
        /// </summary>
        public async Task<bool> ValidateAuthHeaderAsync(string? encryptedAuth)
        {
            if (string.IsNullOrEmpty(encryptedAuth))
                return false;

            try
            {
                var json = _crypto.Decrypt(SharedKey, encryptedAuth);
                var auth = System.Text.Json.JsonSerializer.Deserialize<AuthPayload>(json, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (auth == null || string.IsNullOrEmpty(auth.ClientId) || string.IsNullOrEmpty(auth.SessionToken))
                    return false;

                return await ValidateSessionAsync(auth.ClientId, auth.SessionToken);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Cleanup expired sessions
        /// </summary>
        public async Task CleanupExpiredAsync()
        {
            try
            {
                using var conn = _db.GetConnection();
                conn.Open();
                await conn.ExecuteAsync(
                    "DELETE FROM ClientSessions WHERE ExpiresAt < GETUTCDATE() OR IsActive = 0");
            }
            catch { }
        }
    }

    // ── Models ──
    public class HandshakeRequest
    {
        public string? Encrypted { get; set; }
    }

    public class HandshakePayload
    {
        public string ClientId { get; set; } = "";
        public long Timestamp { get; set; }
    }

    public class AuthPayload
    {
        public string ClientId { get; set; } = "";
        public string SessionToken { get; set; } = "";
    }

    public class ClientSession
    {
        public int Id { get; set; }
        public string ClientId { get; set; } = "";
        public string SessionToken { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; }
    }
}
