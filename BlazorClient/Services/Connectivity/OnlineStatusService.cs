using Microsoft.JSInterop;

namespace BlazorClient.Services.Connectivity;

public sealed class OnlineStatusService : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private IJSObjectReference? _mod;

    public event Action<bool>? StatusChanged;
    public bool IsOnline { get; private set; }

    public OnlineStatusService(IJSRuntime js) => _js = js;

    public async Task InitAsync()
    {
        _mod = await _js.InvokeAsync<IJSObjectReference>("import", "./js/onlineStatus.js");
        IsOnline = await _mod.InvokeAsync<bool>("isOnline");
        var dotnetRef = DotNetObjectReference.Create(this);
        await _mod.InvokeVoidAsync("subscribeOnline", dotnetRef);
    }

    [JSInvokable]
    public void OnStatusChanged(bool isOnline)
    {
        IsOnline = isOnline;
        StatusChanged?.Invoke(isOnline);
    }

    public async ValueTask DisposeAsync()
    {
        if (_mod is not null) await _mod.DisposeAsync();
    }
}
