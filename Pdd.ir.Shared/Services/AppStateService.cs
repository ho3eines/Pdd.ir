namespace Pdd.ir.Shared.Services;

public class AppStateService : IAppStateService
{
    public event Action? OnStateChanged;

    public void NotifyStateChanged()
    {
        OnStateChanged?.Invoke();
    }
}
