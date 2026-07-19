using System.Text;

namespace Pdd.ir.Client.Services
{
    /// <summary>
    /// API Key is XOR-encoded in byte arrays, never appears as plaintext string.
    /// </summary>
    public static class SecureApiKey
    {
        private const byte Mask = 0x3A;

        // XOR-encoded: pdd-ir-ws-2026-secure-key (each byte ^ 0x3A)
        private static readonly byte[] Encoded = {
            0x4a, 0x5e, 0x5e, 0x17, 0x53, 0x48, 0x17, 0x4d, 0x49, 0x17,
            0x08, 0x0a, 0x08, 0x0c, 0x17, 0x49, 0x5f, 0x59, 0x4f, 0x48,
            0x5f, 0x17, 0x51, 0x5f, 0x43
        };

        private static string? _cached;

        public static string GetKey()
        {
            if (_cached != null) return _cached;

            var sb = new StringBuilder(Encoded.Length);
            foreach (var b in Encoded)
                sb.Append((char)(b ^ Mask));

            _cached = sb.ToString();
            return _cached;
        }
    }
}
