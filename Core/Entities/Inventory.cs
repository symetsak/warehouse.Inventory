namespace Core.Entities
{
    public class Inventory
    {
        public int Id { get; set; }
        public required string ScanCode { get; set; }   // από barcode scanner
        public required string Code { get; set; }   // ίδιο με το product.Code
        public required string Action { get; set; }   // “Input” ή “Output”
        public required int WarehouseId { get; set; }
        public  Warehouse Warehouse { get; set; } = null!;
        public required int UserId { get; set; }
        public User User { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }
}

