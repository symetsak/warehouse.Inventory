namespace Shared.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Unit { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalValue { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = null!;
    }
}
