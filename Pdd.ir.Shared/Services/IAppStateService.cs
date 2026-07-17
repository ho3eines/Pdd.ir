namespace Pdd.ir.Shared.Services;

public interface IAppStateService
{
    event Action OnStateChanged;
    int Version { get; }
    void NotifyStateChanged();
}
