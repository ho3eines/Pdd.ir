namespace Pdd.ir.Client.Services;

public interface ITranslateService
{
    Task LoadLanguageAsync(string culture);
    string Text(string key);
    string CurrentLanguage { get; }
    event Action? OnLanguageChanged;
}
