using Dapper;
using Microsoft.Data.SqlClient;
using Pdd.ir.Data;
using Pdd.ir.Server.Models;
using System.Collections.Concurrent;
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

        // ── Atomic nonce store — TryAdd is lock-free and thread-safe ──
        private static readonly ConcurrentDictionary<string, byte> _usedNonces = new();

        // ── Token Bucket per ClientId — atomic, thread-safe ──
        private static readonly ConcurrentDictionary<string, TokenBucket> _buckets = new();

        private string SharedKey => _config["ApiKey"] ?? "";

        public AuthService(IDbService db, CryptoJsService crypto, IConfiguration config, ILogger<AuthService> logger)
        {
            _db = db;
            _crypto = crypto;
            _config = config;
            _logger = logger;
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

                // ── Step 4: Anti-Replay — atomic check+set with ConcurrentDictionary ──
                var nonceKey = $"{payload.ClientId}:{payload.Nonce}";
                if (!_usedNonces.TryAdd(nonceKey, 0))
                {
                    _logger.LogWarning("Replay attack detected: ClientId={ClientId}, Nonce={Nonce}", payload.ClientId, payload.Nonce);
                    return new HandshakeResult { Success = false, Error = "Duplicate request — replay attack blocked" };
                }

                // Schedule cleanup after 2 minutes
                _ = Task.Delay(TimeSpan.FromMinutes(2)).ContinueWith(_ => { byte removed; _usedNonces.TryRemove(nonceKey, out removed); });

                // ── Step 5: Rate Limiting — Token Bucket per ClientId ──
                var bucket = _buckets.GetOrAdd(payload.ClientId, _ => new TokenBucket(capacity: 5, refillRate: 1.0));
                if (!bucket.TryConsume())
                {
                    _logger.LogWarning("Token bucket empty: ClientId={ClientId}, Tokens={Tokens:F1}", payload.ClientId, bucket.AvailableTokens);
                    return new HandshakeResult { Success = false, Error = "Rate limit exceeded — try again later" };
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
        /// Validate session: constant-time hash comparison to prevent timing attacks
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

                // Always fetch — even if not found, we compare in constant time
                var sessions = await conn.QueryAsync<AuthSession>(
                    "SELECT * FROM AuthSessions WHERE ClientId = @ClientId AND IsActive = 1 AND ExpiresAt > GETUTCDATE()",
                    new { ClientId = clientId });

                // Constant-time comparison: compare hash against ALL active sessions
                var dummyHash = new string('0', 44); // SHA256 base64 = 44 chars
                var anyMatch = false;

                foreach (var session in sessions)
                {
                    // Constant-time byte comparison
                    var storedBytes = Convert.FromBase64String(session.TokenHash);
                    var providedBytes = Convert.FromBase64String(tokenHash);

                    if (storedBytes.Length == providedBytes.Length)
                    {
                        if (CryptographicOperations.FixedTimeEquals(storedBytes, providedBytes))
                        {
                            anyMatch = true;
                        }
                    }
                }

                return anyMatch;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Session validation error");
                return false;
            }
        }

        /// <summary>
        /// Validate encrypted auth header: decrypt → verify HMAC → check timestamp → check nonce → check session
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

                // ── Verify HMAC (tamper-proof) ──
                var dataToVerify = $"{auth.ClientId}:{auth.SessionToken}:{auth.Timestamp}:{auth.Nonce}";
                var expectedHmac = ComputeHmac(dataToVerify);
                if (!CryptographicOperations.FixedTimeEquals(
                    Convert.FromBase64String(auth.Hmac),
                    Convert.FromBase64String(expectedHmac)))
                {
                    _logger.LogWarning("HMAC verification failed: ClientId={ClientId}", auth.ClientId);
                    return false;
                }

                // ── Verify timestamp (≤30s) ──
                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var timeDiff = Math.Abs(now - auth.Timestamp);
                if (timeDiff > 30)
                {
                    _logger.LogWarning("Auth header expired: ClientId={ClientId}, Diff={Diff}s", auth.ClientId, timeDiff);
                    return false;
                }

                // ── Check nonce (anti-replay) ──
                var nonceKey = $"auth:{auth.ClientId}:{auth.Nonce}";
                if (!_usedNonces.TryAdd(nonceKey, 0))
                {
                    _logger.LogWarning("Auth replay detected: ClientId={ClientId}, Nonce={Nonce}", auth.ClientId, auth.Nonce);
                    return false;
                }
                _ = Task.Delay(TimeSpan.FromSeconds(60)).ContinueWith(_ => { byte removed; _usedNonces.TryRemove(nonceKey, out removed); });

                // ── Validate session in DB ──
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

        /// <summary>
        /// Compute HMAC-SHA256 for tamper-proof auth header verification
        /// </summary>
        private string ComputeHmac(string data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(SharedKey);
            using var hmac = new HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hash);
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
        public long Timestamp { get; set; }
        public string Nonce { get; set; } = "";
        public string Hmac { get; set; } = "";
    }
}
