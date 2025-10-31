using Core.Entities;
using Infrastructure.Persistence;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebApi.Auth; // IJwtService, TokenGenerator

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IJwtService _jwt;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ApplicationDbContext db, IJwtService jwt, ILogger<AuthController> logger)
    {
        _db = db;
        _jwt = jwt;
        _logger = logger;
    }

    // DTOs 
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
    public record ResetPasswordRequest(string Username, string CurrentPassword, string NewPassword);

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req)
    {
        var user = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username.ToLower() == req.Username.ToLower());
        if (user is null) return Unauthorized(new { reason = "user-not-found" });
        if (!PasswordHasher.Verify(req.Password, user.PasswordHash)) return Unauthorized(new { reason = "bad-password" });

        var accessToken = _jwt.CreateToken(user.Id, user.Username, user.Role, user.FullName, user.Email);
        var accessExp = DateTime.UtcNow.AddMinutes(60);

        var refresh = new RefreshToken
        {
            Token = TokenGenerator.Create(64),
            UserId = user.Id,
            Expires = DateTime.UtcNow.AddDays(7)
        };
        _db.RefreshTokens.Add(refresh);
        await _db.SaveChangesAsync();

        return Ok(new AuthResponse(accessToken, accessExp, user.Username, user.Role, user.FullName, refresh.Token, refresh.Expires));
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<object> Me() => Ok(new
    {
        Name = User.Identity?.Name,
        Role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value,
        Claims = User.Claims.Select(c => new { c.Type, c.Value })
    });

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.RefreshToken)) return Unauthorized();

        var rt = await _db.RefreshTokens.Include(r => r.User)
                                        .FirstOrDefaultAsync(r => r.Token == req.RefreshToken);
        if (rt is null) return Unauthorized(new { reason = "refresh-not-found" });
        if (!rt.IsActive) return Unauthorized(new { reason = "refresh-inactive" });

        var user = rt.User;

        rt.Revoked = DateTime.UtcNow;
        var newRt = new RefreshToken
        {
            Token = TokenGenerator.Create(64),
            UserId = user.Id,
            Expires = DateTime.UtcNow.AddDays(7)
        };
        rt.ReplacedByToken = newRt.Token;
        _db.RefreshTokens.Add(newRt);

        var newAccess = _jwt.CreateToken(user.Id, user.Username, user.Role, user.FullName, user.Email);
        var accessExp = DateTime.UtcNow.AddMinutes(60);

        await _db.SaveChangesAsync();

        return Ok(new AuthResponse(newAccess, accessExp, user.Username, user.Role, user.FullName, newRt.Token, newRt.Expires));
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.RefreshToken)) return Ok();

        var rt = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == req.RefreshToken);
        if (rt is not null && rt.IsActive) rt.Revoked = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
    {
        try
        {
            _logger.LogInformation("[ResetPW] U='{Username}', curLen={CurLen}, newLen={NewLen}",
                req?.Username, req?.CurrentPassword?.Length, req?.NewPassword?.Length);

            if (req is null) return BadRequest(new { message = "Invalid request." });

            var username = (req.Username ?? string.Empty).Trim();
            var current = req.CurrentPassword ?? string.Empty;
            var @new = req.NewPassword ?? string.Empty;

            if (username.Length == 0 || current.Length == 0 || @new.Length == 0)
                return BadRequest(new { message = "Invalid username or password." });
            if (@new.Length < 6)
                return BadRequest(new { message = "New password is too short." });

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
            if (user is null) return BadRequest(new { message = "Invalid username or password." });
            if (!PasswordHasher.Verify(current, user.PasswordHash))
                return BadRequest(new { message = "Invalid username or password." });
            if (current == @new)
                return BadRequest(new { message = "New password must be different from current." });

            user.PasswordHash = PasswordHasher.Hash(@new);

            var now = DateTime.UtcNow;
            var tokens = await _db.RefreshTokens
                .Where(t => t.UserId == user.Id && t.Revoked == null && t.Expires > now)
                .ToListAsync();
            foreach (var t in tokens) t.Revoked = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            _logger.LogInformation("[ResetPW] success for '{Username}'", username);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ResetPW] unexpected error");
            return Problem(title: "Unexpected error.", statusCode: 500);
        }
    }
}
