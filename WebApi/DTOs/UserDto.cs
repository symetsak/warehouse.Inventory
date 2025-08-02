namespace WebApi.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Mobile { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Role { get; set; } = null!;
        // Δεν εκθέτουμε ποτέ το Password στο DTO
    }
}
