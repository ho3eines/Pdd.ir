using Microsoft.AspNetCore.Components;
using Pdd.ir.Shared.Services;

namespace Pdd.ir.Shared.Components;

public abstract class PddComponentBase : ComponentBase
{
    [CascadingParameter] protected IAppStateService AppState { get; set; } = default!;
    private int _lastVersion;

    protected override void OnParametersSet()
    {
        if (AppState.Version != _lastVersion)
        {
            _lastVersion = AppState.Version;
            InvokeAsync(StateHasChanged);
        }
    }
}
