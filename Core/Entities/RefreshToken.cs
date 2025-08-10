namespace Core.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public required string Token { get; set; }          // μοναδικό τυχαίο string
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Expires { get; set; }               // π.χ. +7 ημέρες
        public DateTime? Revoked { get; set; }              // αν γίνει logout/rotation
        public string? ReplacedByToken { get; set; }        // για rotation chain
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public bool IsActive => Revoked == null && !IsExpired;
    }
}
