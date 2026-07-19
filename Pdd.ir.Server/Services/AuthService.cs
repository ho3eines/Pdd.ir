using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Pdd.ir.Data;
using Pdd.ir.Server.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Pdd.ir.Server.Services
{
    public class AuthService
    {
        private readonly IDbService _db;
        private readonly CryptoJsService _crypto;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthService> _logger;
        private readonly IMemoryCache _nonceCache;
        private static readonly Dictionary<string, int> _rateLimiter = new();
        private static readonly object _rateLock = new();

        private string SharedKey => _config["ApiKey"] ?? "";

        public AuthService(IDbService db, CryptoJsService crypto, IConfiguration config, ILogger<AuthService> logger, IMemoryCache nonceCache)
        {
            _db = db;
            _crypto = crypto;
            _config = config;
            _logger = logger;
            _nonceCache = nonceCache;
        }

        /// <summary>
        /// Full handshake flow: decrypt → validate → anti-replay → rate limit → create session → encrypt response
        /// </summary>
        public async Task<HandshakeResult> HandleHandshakeAsync(string encryptedPayload)
        {
            try
            {
                // ── Step 1: Decrypt with shared key ──
                string json;
                try
                {
                    json = _crypto.Decrypt(SharedKey, encryptedPayload);
                }
                catch
                {
                    return new HandshakeResult { Success = false, Error = "Decryption failed — invalid key or corrupted payload" };
                }

                // ── Step 2: Parse { ClientId, Timestamp, Nonce } ──
                var payload = JsonSerializer.Deserialize<HandshakePayload>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (payload == null || string.IsNullOrEmpty(payload.ClientId) || payload.Timestamp == 0 || string.IsNullOrEmpty(payload.Nonce))
                    return new HandshakeResult { Success = false, Error = "Invalid payload — missing ClientId, Timestamp, or Nonce" };

                // ── Step 3: Validate Timestamp (UTC only) ──
                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var timeDiff = now - payload.Timestamp;

                // Reject future timestamps (>5s ahead)
                if (timeDiff < -5)
                {
                    _logger.LogWarning("Future timestamp rejected: ClientId={ClientId}, Diff={Diff}s", payload.ClientId, timeDiff);
                    return new HandshakeResult { Success = false, Error = "Future timestamp not allowed — check your system clock", TimeDiff = timeDiff };
                }

                // Reject expired timestamps (>60s ago)
                if (timeDiff > 60)
                {
                    _logger.LogWarning("Expired request from ClientId={ClientId}, Diff={Diff}s", payload.ClientId, timeDiff);
                    return new HandshakeResult { Success = false, Error = $"Request expired ({timeDiff}s > 60s). Please sync your system clock to UTC.", TimeDiff = timeDiff };
                }

                // ── Step 4: Anti-Replay — check Nonce bound to ClientId + Timestamp ──
                var nonceKey = $"{payload.ClientId}:{payload.Nonce}:{payload.Timestamp}";
                if (_nonceCache.TryGetValue(nonceKey, out _))
                {
                    _logger.LogWarning("Replay attack detected: ClientId={ClientId}, Nonce={Nonce}", payload.ClientId, payload.Nonce);
                    return new HandshakeResult { Success = false, Error = "Duplicate request — replay attack blocked" };
                }

                // Store nonce with 2-minute TTL
                _nonceCache.Set(nonceKey, true, TimeSpan.FromMinutes(2));

                // ── Step 5: Rate Limiting (max 10 handshakes per minute per ClientId) ──
                lock (_rateLock)
                {
                    if (_rateLimiter.TryGetValue(payload.ClientId, out var count))
                    {
                        if (count >= 10)
                        {
                            _logger.LogWarning("Rate limit exceeded: ClientId={ClientId}", payload.ClientId);
                            return new HandshakeResult { Success = false, Error = "Rate limit exceeded — try again later" };
                        }
                        _rateLimiter[payload.ClientId] = count + 1;
                    }
                    else
                    {
                        _rateLimiter[payload.ClientId] = 1;
                    }

                    // Cleanup old entries every 100 entries
                    if (_rateLimiter.Count > 100)
                    {
                        var keysToRemove = _rateLimiter.Keys.Take(50).ToList();
                        foreach (var key in keysToRemove)
                            _rateLimiter.Remove(key);
                    }
                }

                // ── Step 6: Generate Session Token ──
                var tokenBytes = RandomNumberGenerator.GetBytes(32);
                var sessionToken = Convert.ToBase64String(tokenBytes);
                var tokenHash = HashToken(sessionToken);
                var expiresAt = DateTime.UtcNow.AddMinutes(30);

                // ── Step 7: Deactivate old sessions ──
                using (var conn = _db.GetConnection())
                {
                    conn.Open();
                    await conn.ExecuteAsync(
                        "UPDATE AuthSessions SET IsActive = 0 WHERE ClientId = @ClientId AND IsActive = 1",
                        new { ClientId = payload.ClientId });
                }

                // ── Step 8: Insert new session (store HASH only) ──
                using (var conn = _db.GetConnection())
                {
                    conn.Open();
                    await conn.ExecuteAsync(
                        "INSERT INTO AuthSessions (ClientId, TokenHash, CreatedAt, ExpiresAt, IsActive) VALUES (@ClientId, @TokenHash, GETUTCDATE(), @ExpiresAt, 1)",
                        new { ClientId = payload.ClientId, TokenHash = tokenHash, ExpiresAt = expiresAt });
                }

                _logger.LogInformation("Session created: ClientId={ClientId}, Expires={Expires}", payload.ClientId, expiresAt);

                // ── Step 9: Encrypt response { SessionToken, ExpiresAt } ──
                var responsePayload = JsonSerializer.Serialize(new { sessionToken, expiresAt = expiresAt.ToString("O") });
                var encryptedResponse = _crypto.Encrypt(SharedKey, responsePayload);

                return new HandshakeResult { Success = true, EncryptedResponse = encryptedResponse };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Handshake error");
                return new HandshakeResult { Success = false, Error = "Internal server error" };
            }
        }

        /// <summary>
        /// Validate session: hash the token, check DB
        /// </summary>
        public async Task<bool> ValidateSessionAsync(string clientId, string sessionToken)
        {
            try
            {
                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(sessionToken))
                    return false;

                var tokenHash = HashToken(sessionToken);

                using var conn = _db.GetConnection();
                conn.Open();

                var session = await conn.QueryFirstOrDefaultAsync<AuthSession>(
                    "SELECT * FROM AuthSessions WHERE ClientId = @ClientId AND TokenHash = @TokenHash AND IsActive = 1 AND ExpiresAt > GETUTCDATE()",
                    new { ClientId = clientId, TokenHash = tokenHash });

                return session != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Session validation error");
                return false;
            }
        }

        /// <summary>
        /// Validate encrypted auth header: decrypt → { ClientId, SessionToken } → check DB
        /// </summary>
        public async Task<bool> ValidateAuthHeaderAsync(string? encryptedAuth)
        {
            if (string.IsNullOrEmpty(encryptedAuth))
                return false;

            try
            {
                var json = _crypto.Decrypt(SharedKey, encryptedAuth);
                var auth = JsonSerializer.Deserialize<AuthPayload>(json, new JsonSerializerOptions
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
                    "DELETE FROM AuthSessions WHERE ExpiresAt < GETUTCDATE() OR IsActive = 0");
            }
            catch { }
        }

        /// <summary>
        /// SHA256 hash of token for secure storage
        /// </summary>
        public static string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }
    }

    // ── Models ──
    public class HandshakePayload
    {
        public string ClientId { get; set; } = "";
        public long Timestamp { get; set; }
        public string Nonce { get; set; } = "";
    }

    public class HandshakeRequest
    {
        public string? Encrypted { get; set; }
    }

    public class HandshakeResult
    {
        public bool Success { get; set; }
        public string? EncryptedResponse { get; set; }
        public string? Error { get; set; }
        public long? TimeDiff { get; set; }
    }

    public class AuthPayload
    {
        public string ClientId { get; set; } = "";
        public string SessionToken { get; set; } = "";
    }
}
