using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using Pdd.ir.Shared.Models;

namespace Pdd.ir.Shared.Services;

public class EncryptionService : IEncryptionService, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _jsModule;
    private readonly AppSettings _appSettings;
    private bool _isInitialized = false;

    public EncryptionService(IJSRuntime jsRuntime, IOptions<AppSettings> appSettings)
    {
        _jsRuntime = jsRuntime;
        _appSettings = appSettings.Value;
    }

    public async Task InitializeAsync()
    {
        if (!_isInitialized)
        {
            _jsModule ??= await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Pdd.ir.Shared/js/interop.js");
            _isInitialized = true;
        }
    }

    public async Task<string> GenerateRandomKey()
    {
        await EnsureInitializedAsync();
        return await _jsModule!.InvokeAsync<string>("generateRandomKey");
    }

    public async Task<string> EncryptDataAsync(string data, string key = "")
    {
        await EnsureInitializedAsync();
        if (string.IsNullOrEmpty(key)) key = _appSettings.Encryption.Key;
        if (string.IsNullOrEmpty(data)) throw new ArgumentException("Data cannot be null or empty");
        if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty");
        return await _jsModule!.InvokeAsync<string>("encryptData", data, key);
    }

    public async Task<string> DecryptDataAsync(string encryptedData, string key = "")
    {
        await EnsureInitializedAsync();
        if (string.IsNullOrEmpty(key)) key = _appSettings.Encryption.Key;
        if (string.IsNullOrEmpty(encryptedData)) throw new ArgumentException("Encrypted data cannot be null or empty");
        if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty");
        return await _jsModule!.InvokeAsync<string>("decryptData", encryptedData, key);
    }

    private async Task EnsureInitializedAsync()
    {
        if (!_isInitialized) await InitializeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_jsModule != null) await _jsModule.DisposeAsync();
    }
}
