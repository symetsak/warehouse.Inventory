namespace BlazorClient.Services.Offline;

public sealed class InventoryScanLocal
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string? ScanCode { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string Action { get; set; } = default!; // π.χ. "IN" / "OUT" / "COUNT"
    public DateTime Timestamp { get; set; } 
    public int Quantity { get; set; }
    public int WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public string? User { get; set; } = default!;
    public bool Synced { get; set; } = false;
}
