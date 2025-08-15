using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs
{
    public class ResetPasswordDto
    {
        [Required] public string Username { get; set; } = string.Empty;
        [Required] public string CurrentPassword { get; set; } = string.Empty;
        [Required, MinLength(6)] public string NewPassword { get; set; } = string.Empty;
    }
}
