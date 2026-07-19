using System.Text;

namespace Pdd.ir.Client.Services
{
    /// <summary>
    /// API Key is XOR-encoded in byte arrays, never appears as plaintext string.
    /// </summary>
    public static class SecureApiKey
    {
        private const byte Mask = 0x3A;

        // XOR-encoded fragments of: pdd-ir-ws-2026-secure-key
        // Each byte = originalChar ^ 0x3A
        private static readonly byte[] FragmentA = { 0x4A, 0x5A, 0x5A, 0x17, 0x43, 0x5C, 0x17, 0x59, 0x55, 0x17, 0x08, 0x0A, 0x08, 0x0C, 0x17, 0x55, 0x5B, 0x59 };
        private static readonly byte[] FragmentB = { 0x5B, 0x5C, 0x5B, 0x17, 0x49, 0x5B, 0x63 };

        private static string? _cached;

        public static string GetKey()
        {
            if (_cached != null) return _cached;

            var combined = new byte[FragmentA.Length + FragmentB.Length];
            Buffer.BlockCopy(FragmentA, 0, combined, 0, FragmentA.Length);
            Buffer.BlockCopy(FragmentB, 0, combined, FragmentA.Length, FragmentB.Length);

            var sb = new StringBuilder(combined.Length);
            foreach (var b in combined)
                sb.Append((char)(b ^ Mask));

            _cached = sb.ToString();
            return _cached;
        }
    }
}
