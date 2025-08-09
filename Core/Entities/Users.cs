namespace Core.Entities
{
    public class User
    {
        public int Id { get; set; }
        public required string FullName { get; set; }
        public string Mobile { get; set; } = null!;
        public string Email { get; set; } = null!;
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }  
        public required string Role { get; set; }  // π.χ. “Admin”, “Clerk”

        public ICollection<Inventory>? Inventories { get; set; }
    }
}
