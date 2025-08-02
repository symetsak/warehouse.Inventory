namespace Core.Entities
{
    public class Warehouse
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Address { get; set; }

        public ICollection<Product>? Products { get; set; }
        public ICollection<Inventory>? Inventories { get; set; }
    }
}

