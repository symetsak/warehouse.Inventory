using System.Net.Http;
using System.Text;
using BlazorClient.Services.Offline;

namespace BlazorClient.Services.Sync
{
    public sealed class SyncService : ISyncService
    {
        private readonly IOfflineDb _db;
        private readonly HttpClient _http;
        private readonly PendingCounterService _pending;


        public SyncService(IOfflineDb db, HttpClient http, PendingCounterService pending)
        {
            _db = db;
            _http = http;
            _pending = pending;
        }

        public async Task SyncPendingAsync()
        {
            var items = await _db.GetPendingAsync();

            foreach (var item in items.OrderBy(i => i.CreatedAt))
            {
                // Ετοίμασε σωστό HTTP request ανά τύπο
                HttpRequestMessage? req = item.Type switch
                {
                    "CREATE" => new HttpRequestMessage(HttpMethod.Post, item.Endpoint)
                    {
                        Content = new StringContent(item.PayloadJson ?? "{}", Encoding.UTF8, "application/json")
                    },
                    "UPDATE" => new HttpRequestMessage(HttpMethod.Put, item.Endpoint)
                    {
                        Content = new StringContent(item.PayloadJson ?? "{}", Encoding.UTF8, "application/json")
                    },
                    "DELETE" => new HttpRequestMessage(HttpMethod.Delete, item.Endpoint),
                    _ => null
                };

                if (req is null)
                {
                    // Άγνωστος τύπος -> καθάρισέ το για να μη μπλοκάρει
                    await _db.RemovePendingAsync(item.Id);
                    continue;
                }

                HttpResponseMessage res;
                try
                {
                    res = await _http.SendAsync(req);
                }
                catch
                {
                    // Πρόβλημα δικτύου — κράτα το για retry, συνέχισε στα επόμενα
                    continue;
                }

                if (res.IsSuccessStatusCode)
                {
                    // Αν υπάρχει αναφορά στο τοπικό scan, σημείωσέ το ως synced
                    if (!string.IsNullOrWhiteSpace(item.LocalScanId))
                        await _db.MarkSyncedAsync(item.LocalScanId!);

                    await _db.RemovePendingAsync(item.Id);
                }
                else if ((int)res.StatusCode >= 400 && (int)res.StatusCode < 500)
                {
                    // 4xx συνήθως δεν φτιάχνουν με retry (bad request, validation κ.λπ.)
                    // απόφυγε να μπλοκάρει η ουρά — καθάρισέ το
                    await _db.RemovePendingAsync(item.Id);
                }
                else
                {
                    // 5xx: server side, άστο για επόμενη προσπάθεια
                    // (no-op)
                }
            }
        }
    }
}
