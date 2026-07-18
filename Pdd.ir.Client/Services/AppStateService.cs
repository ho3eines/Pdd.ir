namespace Pdd.ir.Client.Services;

public class AppStateService : IAppStateService
{
    public event Action? OnStateChanged;
    public int Version { get; private set; }

    public void NotifyStateChanged()
    {
        Version++;
        OnStateChanged?.Invoke();
    }
}
