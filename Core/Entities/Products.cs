namespace Core.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public required string Code { get; set; }    
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required string Unit { get; set; }    // π.χ. “pcs”, “kg”
        public required int Quantity { get; set; }    // τρέχουσα ποσότητα
        public required decimal Price { get; set; }    // τιμή μονάδας
        public decimal TotalValue { get; set; }    // Price * Quantity 
        public required int WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }
        public ICollection<ProductBarcode> Barcodes { get; set; } = new List<ProductBarcode>();

    }
}

