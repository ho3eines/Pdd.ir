using System.Net.Http.Json;
using Microsoft.JSInterop;

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

    public async Task InitializeAsync()
    {
        try
        {
            var saved = await _js.InvokeAsync<string?>("localStorage.getItem", "app_lang");
            if (!string.IsNullOrEmpty(saved) && saved != _currentLanguage)
            {
                await LoadLanguageAsync(saved);
            }
            else
            {
                await LoadLanguageAsync(_currentLanguage);
            }
        }
        catch
        {
            await LoadLanguageAsync(_currentLanguage);
        }
    }

    public async Task LoadLanguageAsync(string culture)
    {
        _currentLanguage = culture;

        try
        {
            // Use JS fetch to load from static files (not API server)
            var json = await _js.InvokeAsync<string>("eval", 
                $"fetch('/lang/{culture}.json').then(r => r.text())");
            
            if (!string.IsNullOrEmpty(json))
            {
                var loaded = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (loaded != null && loaded.Count > 0)
                    _translations = loaded;
                else
                    _translations = new();
            }
            else
            {
                _translations = new();
            }
        }
        catch
        {
            _translations = new();
        }

        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", "app_lang", culture);
            await _js.InvokeVoidAsync("setDocDirection", culture);
        }
        catch { }

        OnLanguageChanged?.Invoke();
    }

    public string Text(string key)
    {
        return _translations.TryGetValue(key, out var value) ? value : key;
    }
}
