using System.Collections.Concurrent;

namespace Pdd.ir.Server.Services
{
    public class AesKeyStore
    {
        private readonly ConcurrentDictionary<string, string> _keys = new();

        public void SetKey(string username, string aesKey)
        {
            _keys[username] = aesKey;
        }

        public string? GetKey(string username)
        {
            _keys.TryGetValue(username, out var key);
            return key;
        }

        public void RemoveKey(string username)
        {
            _keys.TryRemove(username, out _);
        }

        /// <summary>
        /// Try to decrypt with any stored AES key (for WS requests encrypted client-side)
        /// </summary>
        public string? TryDecryptWithAnyKey(CryptoJsService crypto, string encryptedText)
        {
            foreach (var kvp in _keys)
            {
                try
                {
                    var decrypted = crypto.Decrypt(kvp.Value, encryptedText);
                    if (!string.IsNullOrEmpty(decrypted))
                        return decrypted;
                }
                catch { }
            }
            return null;
        }
    }
}
