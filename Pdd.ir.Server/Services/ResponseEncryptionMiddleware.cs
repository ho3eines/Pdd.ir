using System.IO;
using System.Text;
using System.Text.Json;

namespace Pdd.ir.Server.Services
{
    public class ResponseEncryptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AesKeyStore _keyStore;
        private readonly CryptoJsService _crypto;
        private readonly ILogger<ResponseEncryptionMiddleware> _logger;

        public ResponseEncryptionMiddleware(
            RequestDelegate next,
            AesKeyStore keyStore,
            CryptoJsService crypto,
            ILogger<ResponseEncryptionMiddleware> logger)
        {
            _next = next;
            _keyStore = keyStore;
            _crypto = crypto;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // ── Only encrypt /api/ responses ──
            if (!context.Request.Path.StartsWithSegments("/api"))
            {
                await _next(context);
                return;
            }

            // ── Skip encryption for auth/login (needs plaintext AES key in response) ──
            if (context.Request.Path.Value?.Contains("/auth/login") == true)
            {
                await _next(context);
                return;
            }

            // ── Get username from X-Username header or JWT ──
            var username = context.Request.Headers["X-Username"].FirstOrDefault()
                ?? context.User?.Identity?.Name;

            if (string.IsNullOrEmpty(username))
            {
                await _next(context);
                return;
            }

            var aesKey = _keyStore.GetKey(username);
            if (string.IsNullOrEmpty(aesKey))
            {
                await _next(context);
                return;
            }

            // ── Capture original response body ──
            var originalBody = context.Response.Body;
            using var newBody = new MemoryStream();
            context.Response.Body = newBody;

            await _next(context);

            // ── Read response and encrypt ──
            newBody.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(newBody).ReadToEndAsync();

            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300
                && !string.IsNullOrEmpty(responseBody))
            {
                try
                {
                    var encrypted = _crypto.Encrypt(aesKey, responseBody);
                    var encryptedResponse = JsonSerializer.Serialize(new { encrypted = true, data = encrypted });
                    var encryptedBytes = Encoding.UTF8.GetBytes(encryptedResponse);

                    await originalBody.WriteAsync(encryptedBytes);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Encryption failed: {Error}", ex.Message);
                    var bytes = Encoding.UTF8.GetBytes(responseBody);
                    await originalBody.WriteAsync(bytes);
                }
            }
            else
            {
                newBody.Seek(0, SeekOrigin.Begin);
                await newBody.CopyToAsync(originalBody);
            }

            context.Response.Body = originalBody;
        }
    }
}
