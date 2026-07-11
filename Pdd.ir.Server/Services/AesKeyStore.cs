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
    }
}
