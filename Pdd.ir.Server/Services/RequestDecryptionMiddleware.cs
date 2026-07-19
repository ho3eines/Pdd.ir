using System.IO;
using System.Text;
using System.Text.Json;

namespace Pdd.ir.Server.Services
{
    /// <summary>
    /// Middleware: decrypt incoming encrypted request bodies using the SharedKey.
    /// Client sends: { "encrypted": true, "data": "base64..." }
    /// Server decrypts and replaces HttpContext.Request.Body with the decrypted JSON.
    /// </summary>
    public class RequestDecryptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly CryptoJsService _crypto;
        private readonly IConfiguration _config;
        private readonly ILogger<RequestDecryptionMiddleware> _logger;

        private string SharedKey => _config["ApiKey"] ?? "";

        public RequestDecryptionMiddleware(
            RequestDelegate next,
            CryptoJsService crypto,
            IConfiguration config,
            ILogger<RequestDecryptionMiddleware> logger)
        {
            _next = next;
            _crypto = crypto;
            _config = config;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // ── Only decrypt /api/ POST/PUT requests ──
            if (!context.Request.Path.StartsWithSegments("/api") ||
                (context.Request.Method != "POST" && context.Request.Method != "PUT"))
            {
                await _next(context);
                return;
            }

            // ── Skip auth endpoints (handshake uses shared key directly) ──
            var path = context.Request.Path.Value ?? "";
            if (path.Contains("/auth/handshake") || path.Contains("/auth/login"))
            {
                await _next(context);
                return;
            }

            // ── Read original body ──
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            if (string.IsNullOrEmpty(body))
            {
                await _next(context);
                return;
            }

            try
            {
                var doc = JsonSerializer.Deserialize<JsonElement>(body);

                // ── Check if body is encrypted ──
                if (doc.TryGetProperty("encrypted", out var isEncrypted) && isEncrypted.GetBoolean())
                {
                    var encryptedData = doc.GetProperty("data").GetString();
                    if (string.IsNullOrEmpty(encryptedData))
                    {
                        await _next(context);
                        return;
                    }

                    // ── Decrypt with SharedKey ──
                    var decryptedJson = _crypto.Decrypt(SharedKey, encryptedData);

                    // ── Replace request body with decrypted JSON ──
                    var decryptedBytes = Encoding.UTF8.GetBytes(decryptedJson);
                    context.Request.Body = new MemoryStream(decryptedBytes);

                    _logger.LogDebug("Request decrypted: {Path}", path);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Request decryption failed for {Path}: {Error}", path, ex.Message);
                // Continue with original body
            }

            await _next(context);
        }
    }
}
