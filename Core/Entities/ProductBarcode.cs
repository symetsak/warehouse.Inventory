namespace Core.Entities;

public class ProductBarcode
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Code { get; set; } = null!;
    public string Type { get; set; } = "CODE128"; // EAN13/UPC/QR/CODE128...
    public bool IsPrimary { get; set; } = true;

    public Product Product { get; set; } = null!;
}
