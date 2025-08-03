namespace Shared.DTOs
{
    public class CreateProductDto
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Unit { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public int WarehouseId { get; set; }
        public decimal TotalValue { get; internal set; }
    }
}
