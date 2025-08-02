namespace Core.Entities
{
    public class User
    {
        public int Id { get; set; }
        public required string FullName { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }  // αργότερα hashe θα το κάνουμε
        public required string Role { get; set; }  // π.χ. “Admin”, “Clerk”

        public required ICollection<Inventory> Inventories { get; set; }
    }
}
