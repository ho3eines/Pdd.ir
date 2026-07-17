using Microsoft.AspNetCore.Components;

namespace Pdd.ir.Shared.Services;

public interface IModalService
{
    event Action<Models.ModalModel> OnShow;
    event Action<string> OnClose;

    Task<object?> Show<TComponent>(string title, Dictionary<string, object>? parameters = null, bool closeButton = true, string? modalSize = null) where TComponent : ComponentBase;

    Task<object?> Show(Type componentType, string title, Dictionary<string, object>? parameters = null, bool closeButton = true, string? modalSize = null);

    Task<TResult?> ShowAsync<TResult, TComponent>(string title, Dictionary<string, object>? parameters = null, bool closeButton = true, string? modalSize = null)
        where TComponent : ComponentBase;

    void Close(object? result = null, string? modalId = null);
    void CloseAll();
    Task InitializeAsync();
}
