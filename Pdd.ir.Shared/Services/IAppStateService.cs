namespace Pdd.ir.Shared.Services;

public interface IAppStateService
{
    event Action OnStateChanged;
    void NotifyStateChanged();
}
