namespace WebApi.DTOs
{
    public class CreateUserDto
    {
        public string FullName { get; set; } = null!;
        public string Mobile { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;  // αργότερα θα το hashάρουμε
        public string Role { get; set; } = null!;
    }
}
