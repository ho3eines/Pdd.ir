namespace Pdd.ir.Client.Services;

public interface IAlertService
{
    Task ShowSuccessAsync(string title, string message, int duration = 5000);
    Task ShowWarningAsync(string title, string message, int duration = 5000);
    Task ShowErrorAsync(string title, string message, int duration = 5000);
    Task ShowInfoAsync(string title, string message, int duration = 5000);
    Task ShowCustomAsync(string type, string title, string message, int duration = 5000);
    Task HideAllAsync();
}
