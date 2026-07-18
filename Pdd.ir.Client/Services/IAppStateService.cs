namespace Pdd.ir.Client.Services;

public interface IAppStateService
{
    event Action OnStateChanged;
    int Version { get; }
    void NotifyStateChanged();
}
