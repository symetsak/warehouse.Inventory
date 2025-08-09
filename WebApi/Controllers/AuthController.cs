using Core.Entities;
using Infrastructure.Persistence;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Auth;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IJwtService _jwt;

    public AuthController(ApplicationDbContext db, IJwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public record LoginRequest(string Username, string Password);
    public record AuthResponse(string AccessToken, DateTime ExpiresAt, string UserName, string Role, string FullName);

    /// <summary>
    /// Login: δίνει Access Token (JWT) αν τα στοιχεία είναι σωστά.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req)
    {
        // βρίσκουμε τον χρήστη (username case-insensitive)
        var user = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username.ToLower() == req.Username.ToLower());

        if(user is null)
        return Unauthorized(new { reason = "user-not-found" });

        var ok = Infrastructure.Security.PasswordHasher.Verify(req.Password, user.PasswordHash);
        if (!ok)
            return Unauthorized(new { reason = "bad-password" });

        // έκδοση JWT
        var token = _jwt.CreateToken(user.Id, user.Username, user.Role, user.FullName, user.Email);

        // διάρκεια από τα options (την ίδια που έχεις στο appsettings)
        var expiresAt = DateTime.UtcNow.AddMinutes(60); // αν θες, πέρασέ το από τα JwtOptions

        return Ok(new AuthResponse(token, expiresAt, user.Username, user.Role, user.FullName));
    }

    /// <summary>
    /// Επιστρέφει απλά τα claims του τρέχοντος χρήστη για δοκιμή.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public ActionResult<object> Me()
    {
        return Ok(new
        {
            Name = User.Identity?.Name,
            Role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value,
            Claims = User.Claims.Select(c => new { c.Type, c.Value })
        });
    }
}
