using System.Threading.Tasks;

namespace BlazorClient.Services.Sync;

public interface ISyncService
{
    Task SyncPendingAsync();
}
