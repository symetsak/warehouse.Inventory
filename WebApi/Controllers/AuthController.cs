using Core.Entities;
using Infrastructure.Persistence;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Auth; // TokenGenerator + IJwtService

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

    // ===== DTOs =====
    public record LoginRequest(string Username, string Password);
    public record RefreshRequest(string RefreshToken);

    public record AuthResponse(
        string AccessToken,
        DateTime ExpiresAt,
        string UserName,
        string Role,
        string FullName,
        string RefreshToken,
        DateTime RefreshExpiresAt
    );

    /// <summary>Login: επιστρέφει access + refresh tokens.</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req)
    {
        var user = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username.ToLower() == req.Username.ToLower());

        if (user is null)
            return Unauthorized(new { reason = "user-not-found" });

        if (!PasswordHasher.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { reason = "bad-password" });

        // Access token (60’ – άλλαξε το αν το παίρνεις από JwtOptions)
        var accessToken = _jwt.CreateToken(user.Id, user.Username, user.Role, user.FullName, user.Email);
        var accessExp = DateTime.UtcNow.AddMinutes(60);

        // Refresh token (7 ημέρες)
        var refresh = new RefreshToken
        {
            Token = TokenGenerator.Create(64),
            UserId = user.Id,
            Expires = DateTime.UtcNow.AddDays(7)
        };
        _db.RefreshTokens.Add(refresh);
        await _db.SaveChangesAsync();

        return Ok(new AuthResponse(
            accessToken, accessExp,
            user.Username, user.Role, user.FullName,
            refresh.Token, refresh.Expires
        ));
    }

    /// <summary>Επιστρέφει claims του τρέχοντος χρήστη.</summary>
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

    /// <summary>Ανανέωση access token με refresh token (rotation, μια χρήση ανά refresh).</summary>
    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.RefreshToken))
            return Unauthorized();

        var rt = await _db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == req.RefreshToken);

        if (rt is null)
            return Unauthorized(new { reason = "refresh-not-found" });

        if (!rt.IsActive)
            return Unauthorized(new { reason = "refresh-inactive" });

        var user = rt.User;

        // Rotation: ακύρωσε παλιό, φτιάξε νέο
        rt.Revoked = DateTime.UtcNow;

        var newRt = new RefreshToken
        {
            Token = TokenGenerator.Create(64),
            UserId = user.Id,
            Expires = DateTime.UtcNow.AddDays(7)
        };
        rt.ReplacedByToken = newRt.Token;
        _db.RefreshTokens.Add(newRt);

        // Νέο access
        var newAccess = _jwt.CreateToken(user.Id, user.Username, user.Role, user.FullName, user.Email);
        var accessExp = DateTime.UtcNow.AddMinutes(60);

        await _db.SaveChangesAsync();

        return Ok(new AuthResponse(
            newAccess, accessExp,
            user.Username, user.Role, user.FullName,
            newRt.Token, newRt.Expires
        ));
    }

    /// <summary>Logout: αναιρεί συγκεκριμένο refresh token.</summary>
    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.RefreshToken))
            return Ok();

        var rt = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == req.RefreshToken);
        if (rt is null) return Ok();

        if (rt.IsActive)
            rt.Revoked = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok();
    }
}
