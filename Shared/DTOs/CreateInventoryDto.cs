using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs
{
    public class CreateInventoryDto
    {
        public string? ScanCode { get; set; }
        public string Code { get; set; } = null!;
        public string Action { get; set; } = null!; // "Input" or "Output"
        public int WarehouseId { get; set; }
        public int UserId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
