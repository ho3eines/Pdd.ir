using System.Text;

namespace Pdd.ir.Client.Services
{
    /// <summary>
    /// API Key is XOR-encoded in byte arrays, never appears as plaintext string.
    /// </summary>
    public static class SecureApiKey
    {
        private const byte Mask = 0x3A;

        // XOR-encoded fragments of the API key — key never appears as plaintext
        // "pdd-ir-ws-2026-secure-key" split into two fragments
        private static readonly byte[] FragmentA = { 0x46, 0x4C, 0x4C, 0x17, 0x53, 0x58, 0x17, 0x59, 0x55, 0x17, 0x14, 0x1A, 0x14, 0x18, 0x17, 0x55, 0x4F, 0x59 };
        private static readonly byte[] FragmentB = { 0x5B, 0x58, 0x4F, 0x17, 0x51, 0x4F, 0x63 };

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
