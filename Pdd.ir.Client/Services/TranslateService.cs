using System.Net.Http.Json;

namespace Pdd.ir.Client.Services;

public class TranslateService : ITranslateService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;
    private Dictionary<string, string> _translations = new();
    private string _currentLanguage = "fa";

    public string CurrentLanguage => _currentLanguage;
    public event Action? OnLanguageChanged;

    public TranslateService(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js = js;
    }

    public async Task LoadLanguageAsync(string culture)
    {
        _currentLanguage = culture;

        try
        {
            var json = await _http.GetStringAsync($"lang/{culture}.json");
            _translations = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
        }
        catch
        {
            _translations = new();
        }

        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", "app_lang", culture);
            await _js.InvokeVoidAsync("eval", $"document.documentElement.dir='{(culture == "fa" ? "rtl" : "ltr")}';document.documentElement.lang='{culture}'");
        }
        catch { }

        OnLanguageChanged?.Invoke();
    }

    public string Text(string key)
    {
        return _translations.TryGetValue(key, out var value) ? value : key;
    }
}
