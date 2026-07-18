using Microsoft.JSInterop;

namespace Pdd.ir.Client.Services
{
    public class EncryptionService
    {
        private readonly IJSRuntime _js;
        private string? _aesKey;

        public EncryptionService(IJSRuntime js)
        {
            _js = js;
        }

        public void SetKey(string aesKey)
        {
            _aesKey = aesKey;
        }

        public string? GetKey() => _aesKey;

        public void ClearKey()
        {
            _aesKey = null;
        }

        public async Task<string> EncryptAsync(string plainText)
        {
            if (string.IsNullOrEmpty(_aesKey))
                throw new InvalidOperationException("AES key not set");

            return await _js.InvokeAsync<string>("CryptoUtils.encryptData", plainText, _aesKey);
        }

        public async Task<string> DecryptAsync(string ciphertext)
        {
            if (string.IsNullOrEmpty(_aesKey))
                throw new InvalidOperationException("AES key not set");

            return await _js.InvokeAsync<string>("CryptoUtils.decryptData", ciphertext, _aesKey);
        }
    }
}
