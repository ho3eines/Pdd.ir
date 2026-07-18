namespace Pdd.ir.Client.Services;

public interface IClientStorageService
{
    Task SetLocalAsync<T>(string key, T value);
    Task<T?> GetLocalAsync<T>(string key);
    Task RemoveLocalAsync(string key);
    Task ClearLocalAsync();
    Task SetLocalEncryptedAsync<T>(string key, T value, string secretKey);
    Task<T?> GetLocalEncryptedAsync<T>(string key, string secretKey);
    Task SetSessionAsync<T>(string key, T value);
    Task<T?> GetSessionAsync<T>(string key);
    Task RemoveSessionAsync(string key);
    Task ClearSessionAsync();
    Task SetCookieAsync(string key, string value, int days = 30);
    Task<string?> GetCookieAsync(string key);
    Task RemoveCookieAsync(string key);
    Task SetSecureCookieAsync(string key, string value, int days = 30);
    string EncryptString(string plainText, string key);
    string DecryptString(string cipherText, string key);
}
