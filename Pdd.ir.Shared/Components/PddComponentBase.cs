using Microsoft.AspNetCore.Components;
using Pdd.ir.Shared.Services;

namespace Pdd.ir.Shared.Components;

public abstract class PddComponentBase : ComponentBase, IDisposable
{
    [Inject] protected IAppStateService AppState { get; set; } = default!;

    protected override void OnInitialized()
    {
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
