namespace Shared.DTOs
{
    public class InventoryDto
    {
        public int Id { get; set; }
        public string? ScanCode { get; set; }
        public string Code { get; set; } = null!;
        public string Action { get; set; } = null!;
        public int Quantity { get; set; }
        public DateTime Timestamp { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = null!;
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
    }
}
