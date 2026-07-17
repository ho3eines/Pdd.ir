using Microsoft.AspNetCore.Components;
using Pdd.ir.Shared.Services;

namespace Pdd.ir.Shared.Components;

public abstract class PddComponentBase : ComponentBase, IDisposable
{
    [Inject] protected IAppStateService AppState { get; set; } = default!;

    private bool _subscribed;

    protected override void OnInitialized()
    {
        Subscribe();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (!_subscribed) Subscribe();
    }

    private void Subscribe()
    {
        if (_subscribed) return;
        _subscribed = true;
        AppState.OnStateChanged += OnStateChanged;
    }

    private void OnStateChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        AppState.OnStateChanged -= OnStateChanged;
    }
}
