namespace Core.Entities
{
    public class Warehouse
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Address { get; set; }

        public required ICollection<Product> Products { get; set; }
        public required ICollection<Inventory> Inventories { get; set; }
    }
}

