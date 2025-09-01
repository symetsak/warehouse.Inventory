namespace BlazorClient.Services.Offline;

public sealed class PendingItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Type { get; set; } = "CREATE"; // ή UPDATE/DELETE
    public string Endpoint { get; set; } = "/api/inventory";
    public string PayloadJson { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? LocalScanId { get; set; }
}
