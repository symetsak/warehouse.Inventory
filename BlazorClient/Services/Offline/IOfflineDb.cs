using System.Threading.Tasks;

namespace BlazorClient.Services.Offline;

public interface IOfflineDb
{
    Task SaveScanAsync(InventoryScanLocal scan, bool enqueueForSync);
    Task<List<InventoryScanLocal>> GetScansAsync(int? warehouseId = null);
    Task EnqueueAsync(PendingItem item);
    Task<List<PendingItem>> GetPendingAsync();
    Task RemovePendingAsync(string id);
    Task MarkSyncedAsync(string scanId);
    Task PurgeScansExceptTodayAsync();
}
