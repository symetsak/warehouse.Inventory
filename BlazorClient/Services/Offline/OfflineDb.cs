using Microsoft.JSInterop;
using System.Text.Json;
using BlazorClient.Services.Sync;

namespace BlazorClient.Services.Offline
{
    public sealed class OfflineDb : IOfflineDb, IAsyncDisposable
    {
        private readonly IJSRuntime _js;
        private readonly PendingCounterService _pending;
        private IJSObjectReference? _mod;

        public OfflineDb(IJSRuntime js, PendingCounterService pending)
        {
            _js = js;
            _pending = pending;
        }

        private async Task<IJSObjectReference> Module()
        {
            if (_mod is null)
                _mod = await _js.InvokeAsync<IJSObjectReference>("import", "./js/wmsdb.js");
            return _mod;
        }

        public async Task SaveScanAsync(InventoryScanLocal scan, bool enqueueForSync /*ignored*/)
        {
            var mod = await Module();
            await mod.InvokeVoidAsync("put", "inventory_scans", scan);
        }

        public async Task<List<InventoryScanLocal>> GetScansAsync(int? warehouseId = null)
        {
            var mod = await Module();
            var all = await mod.InvokeAsync<InventoryScanLocal[]>("getAll", "inventory_scans");
            var list = all?.ToList() ?? new List<InventoryScanLocal>();
            return warehouseId is null ? list : list.Where(x => x.WarehouseId == warehouseId.Value).ToList();
        }

        public async Task EnqueueAsync(PendingItem item)
        {
            var mod = await Module();
            await mod.InvokeVoidAsync("put", "pending_sync", item);
            _pending.Increment(); // <<< ΑΜΕΣΗ ενημέρωση badge
        }

        public async Task<List<PendingItem>> GetPendingAsync()
        {
            var mod = await Module();
            var all = await mod.InvokeAsync<PendingItem[]>("getAll", "pending_sync");
            return all?.OrderBy(x => x.CreatedAt).ToList() ?? new List<PendingItem>();
        }

        public async Task RemovePendingAsync(string id)
        {
            var mod = await Module();
            await mod.InvokeVoidAsync("deleteKey", "pending_sync", id);
            _pending.Decrement(); // <<< ΑΜΕΣΗ ενημέρωση badge
        }

        public async Task MarkSyncedAsync(string scanId)
        {
            var mod = await Module();
            var rec = await mod.InvokeAsync<InventoryScanLocal?>("get", "inventory_scans", scanId);
            if (rec is null) return;

            rec.Synced = true;
            await mod.InvokeVoidAsync("put", "inventory_scans", rec);
        }

        public async Task PurgeScansExceptTodayAsync()
        {
            var mod = await Module();
            var all = await mod.InvokeAsync<InventoryScanLocal[]>("getAll", "inventory_scans");
            if (all is null || all.Length == 0) return;

            var today = DateOnly.FromDateTime(DateTime.Now);
            foreach (var s in all)
            {
                if (DateOnly.FromDateTime(s.Timestamp) != today)
                {
                    await mod.InvokeVoidAsync("deleteKey", "inventory_scans", s.Id);
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            try { if (_mod is not null) await _mod.DisposeAsync(); }
            catch { /* ignore */ }
        }
    }
}
