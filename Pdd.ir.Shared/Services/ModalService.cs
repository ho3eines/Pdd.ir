using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Pdd.ir.Shared.Models;

namespace Pdd.ir.Shared.Services;

public class ModalService : IModalService, IAsyncDisposable
{
    public event Action<ModalModel> OnShow = delegate { };
    public event Action<string> OnClose = delegate { };

    private TaskCompletionSource<object?>? _tcs;
    private string? _currentModalId;
    private readonly IJSRuntime _jsRuntime;
    private bool _isInitialized = false;
    private DotNetObjectReference<ModalService>? _dotNetHelper;

    public ModalService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        if (!_isInitialized)
        {
            _dotNetHelper = DotNetObjectReference.Create(this);
            await InjectStyles();
            await InjectScripts();
            _isInitialized = true;
        }
    }

    private async Task InjectStyles()
    {
        var css = @"/* ── Modal Container ── */
.modal-container{position:fixed;top:0;left:0;width:100%;height:100%;z-index:1050;display:flex;align-items:center;justify-content:center;padding:16px}
/* ── Backdrop ── */
.modal-backdrop{position:fixed;top:0;left:0;width:100%;height:100%;background:rgba(0,0,0,0.6);backdrop-filter:blur(12px);-webkit-backdrop-filter:blur(12px);opacity:0;transition:opacity 0.4s cubic-bezier(0.23,1,0.32,1)}
.modal-backdrop.show{opacity:1}
/* ── Dialog ── */
.modal-dialog{transform:scale(0.85) translateY(30px);opacity:0;transition:all 0.4s cubic-bezier(0.34,1.56,0.64,1);max-width:90%;margin:1.75rem auto;z-index:1060;filter:blur(4px)}
.modal-dialog.show{transform:scale(1) translateY(0);opacity:1;filter:blur(0)}
/* ── Content (Glassmorphism) ── */
.modal-content{border:none;border-radius:20px;box-shadow:0 25px 80px rgba(0,0,0,0.4),0 0 0 1px rgba(255,255,255,0.08),inset 0 1px 0 rgba(255,255,255,0.1);background:rgba(15,20,30,0.85);backdrop-filter:blur(24px) saturate(180%);-webkit-backdrop-filter:blur(24px) saturate(180%);display:flex;flex-direction:column;max-height:calc(100vh - 48px);overflow:visible;position:relative}
.modal-content::before{content:'';position:absolute;top:-1px;left:20%;right:20%;height:1px;background:linear-gradient(90deg,transparent,rgba(255,255,255,0.15),transparent);z-index:1}
.modal-content::after{content:'';position:absolute;inset:0;border-radius:20px;pointer-events:none;background:linear-gradient(135deg,rgba(13,110,253,0.05) 0%,transparent 40%,rgba(248,28,229,0.03) 100%);z-index:0}
/* ── Header ── */
.modal-header{display:flex;justify-content:space-between;align-items:center;padding:20px 24px;border-bottom:1px solid rgba(255,255,255,0.06);border-top-left-radius:20px;border-top-right-radius:20px;background:rgba(255,255,255,0.03);position:relative;z-index:1}
.modal-title{margin:0;font-weight:700;font-size:16px;flex:1;color:#fff}
/* ── Close Button ── */
.btn-close-rtl{position:relative;width:36px;height:36px;border-radius:10px;background:rgba(255,255,255,0.06);border:1px solid rgba(255,255,255,0.08);color:rgba(255,255,255,0.5);display:flex;align-items:center;justify-content:center;cursor:pointer;font-size:16px;transition:all 0.25s cubic-bezier(0.23,1,0.32,1);flex-shrink:0;margin-left:12px}
.btn-close-rtl:hover{background:rgba(239,68,68,0.15);border-color:rgba(239,68,68,0.3);color:#ef4444;transform:rotate(90deg) scale(1.1)}
.btn-close-rtl:active{transform:rotate(90deg) scale(0.9)}
/* ── Body ── */
.modal-body{flex:1;min-height:0;overflow-y:auto;overscroll-behavior:contain;padding:24px;position:relative;z-index:1}
.modal-body::-webkit-scrollbar{width:6px}
.modal-body::-webkit-scrollbar-track{background:transparent}
.modal-body::-webkit-scrollbar-thumb{background:rgba(255,255,255,0.1);border-radius:3px}
.modal-body::-webkit-scrollbar-thumb:hover{background:rgba(255,255,255,0.2)}
/* ── Sizes ── */
.modal-sm{max-width:420px}
.modal-lg{max-width:800px}
.modal-xl{max-width:1140px}
/* ── Dropdown fix ── */
.modal-content .dropdown-menu{position:fixed;z-index:1070;min-width:280px;max-width:calc(100vw - 48px);background:rgba(15,20,30,0.95);border:1px solid rgba(255,255,255,0.1);backdrop-filter:blur(20px);border-radius:12px;box-shadow:0 20px 60px rgba(0,0,0,0.5)}
/* ── Light Mode ── */
[data-theme=""light""] .modal-content{background:rgba(255,255,255,0.92);box-shadow:0 25px 80px rgba(0,0,0,0.15),0 0 0 1px rgba(0,0,0,0.05)}
[data-theme=""light""] .modal-content::before{background:linear-gradient(90deg,transparent,rgba(0,0,0,0.05),transparent)}
[data-theme=""light""] .modal-content::after{background:linear-gradient(135deg,rgba(13,110,253,0.03) 0%,transparent 40%,rgba(248,28,229,0.02) 100%)}
[data-theme=""light""] .modal-header{background:rgba(0,0,0,0.02);border-bottom-color:rgba(0,0,0,0.06)}
[data-theme=""light""] .modal-title{color:#1a1a2e}
[data-theme=""light""] .btn-close-rtl{background:rgba(0,0,0,0.04);border-color:rgba(0,0,0,0.08);color:rgba(0,0,0,0.4)}
[data-theme=""light""] .btn-close-rtl:hover{background:rgba(239,68,68,0.1);border-color:rgba(239,68,68,0.2);color:#ef4444}
[data-theme=""light""] .modal-body::-webkit-scrollbar-thumb{background:rgba(0,0,0,0.1)}
[data-theme=""light""] .modal-content .dropdown-menu{background:rgba(255,255,255,0.95);border-color:rgba(0,0,0,0.08);box-shadow:0 20px 60px rgba(0,0,0,0.12)}";

        var jsCode = "if(!document.getElementById('pdd-modal-styles')){" +
            "var s=document.createElement('style');s.id='pdd-modal-styles';s.textContent=`" + css + "`;document.head.appendChild(s);}";

        await _jsRuntime.InvokeVoidAsync("eval", jsCode);
    }

    private async Task InjectScripts()
    {
        var script = @"window.pddModal={dotNetHelper:null,initialize:function(d){this.dotNetHelper=d}," +
            "setupModalEvents:function(id){" +
            "var b=document.querySelector('.modal-backdrop[data-modal-id=\"'+id+'\"]');" +
            "if(b)b.onclick=function(){dotNetHelper.invokeMethodAsync('HandleBackdropClick',id)};" +
            "var c=document.querySelector('.btn-close-rtl[data-modal-id=\"'+id+'\"]');" +
            "if(c)c.onclick=function(){dotNetHelper.invokeMethodAsync('HandleCloseClick',id)};}," +
            "removeModalEvents:function(id){" +
            "var b=document.querySelector('.modal-backdrop[data-modal-id=\"'+id+'\"]');" +
            "if(b)b.onclick=null;" +
            "var c=document.querySelector('.btn-close-rtl[data-modal-id=\"'+id+'\"]');" +
            "if(c)c.onclick=null;}};";

        await _jsRuntime.InvokeVoidAsync("eval", script);
        await _jsRuntime.InvokeVoidAsync("pddModal.initialize", _dotNetHelper);
    }

    public async Task<object?> Show<TComponent>(string title, Dictionary<string, object>? parameters = null, bool closeButton = true, string? modalSize = null) where TComponent : ComponentBase
    {
        if (!_isInitialized) await InitializeAsync();

        _tcs = new TaskCompletionSource<object?>();

        var modal = new ModalModel
        {
            ComponentType = typeof(TComponent),
            Title = title,
            Parameters = parameters ?? new Dictionary<string, object>(),
            Show = true,
            ModalSize = modalSize ?? "modal-lg",
            CloseButton = closeButton
        };

        _currentModalId = modal.Id;
        OnShow?.Invoke(modal);

        await Task.Delay(50);
        await _jsRuntime.InvokeVoidAsync("pddModal.setupModalEvents", modal.Id);

        return await _tcs.Task;
    }

    public async Task<object?> Show(Type componentType, string title, Dictionary<string, object>? parameters = null, bool closeButton = true, string? modalSize = null)
    {
        if (!_isInitialized) await InitializeAsync();

        _tcs = new TaskCompletionSource<object?>();

        var modal = new ModalModel
        {
            ComponentType = componentType,
            Title = title,
            Parameters = parameters ?? new Dictionary<string, object>(),
            Show = true,
            ModalSize = modalSize ?? "modal-lg",
            CloseButton = closeButton
        };

        _currentModalId = modal.Id;
        OnShow?.Invoke(modal);

        await Task.Delay(50);
        await _jsRuntime.InvokeVoidAsync("pddModal.setupModalEvents", modal.Id);

        return await _tcs.Task;
    }

    public async Task<TResult?> ShowAsync<TResult, TComponent>(string title, Dictionary<string, object>? parameters = null, bool closeButton = true, string? modalSize = null)
        where TComponent : ComponentBase
    {
        var result = await Show<TComponent>(title, parameters, closeButton, modalSize);
        return result is TResult typed ? typed : default;
    }

    [JSInvokable]
    public async Task HandleBackdropClick(string modalId) => await CloseModal(modalId);

    [JSInvokable]
    public async Task HandleCloseClick(string modalId) => await CloseModal(modalId);

    private async Task CloseModal(string modalId)
    {
        await _jsRuntime.InvokeVoidAsync("pddModal.removeModalEvents", modalId);
        OnClose?.Invoke(modalId);
    }

    public void Close(object? result = null, string? modalId = null)
    {
        _tcs?.TrySetResult(result);
        var targetModalId = modalId ?? _currentModalId;
        if (targetModalId != null) OnClose?.Invoke(targetModalId);
        _currentModalId = null;
    }

    public void CloseAll()
    {
        _tcs?.TrySetResult(null);
        OnClose?.Invoke("all");
        _currentModalId = null;
    }

    public async ValueTask DisposeAsync()
    {
        _dotNetHelper?.Dispose();
    }
}
