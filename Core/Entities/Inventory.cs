namespace Core.Entities
{
    public class Inventory
    {
        public int Id { get; set; }
        public required string ScanCode { get; set; }   // από barcode scanner
        public required string Code { get; set; }   // ίδιο με το product.Code
        public required string Action { get; set; }   // “Input” ή “Output”
        public required int WarehouseId { get; set; }
        public required Warehouse Warehouse { get; set; }
        public required int UserId { get; set; }
        public required User User { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

