namespace Pdd.ir.Client.Models;

public class AppSettings
{
    public ApiSettings ApiSettings { get; set; } = new();
    public EncryptionSettings Encryption { get; set; } = new();
}

public class ApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string APIKey { get; set; } = string.Empty;
    public string Encryption { get; set; } = string.Empty;
}

public class EncryptionSettings
{
    public string Key { get; set; } = string.Empty;
}
