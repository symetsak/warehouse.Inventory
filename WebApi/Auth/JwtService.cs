using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace WebApi.Auth;

public class JwtService : IJwtService
{
    private readonly JwtOptions _opt;
    public JwtService(IOptions<JwtOptions> opt) => _opt = opt.Value;

    public string CreateToken(int userId, string username, string role, string? fullName = null, string? email = null)
    {
        // 1) Μην κάνεις αυτόματα remap ονομάτων claims
        JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, username),

            // Name / Role όπως τα περιμένει το ASP.NET
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, role),

            // 2) καθαρό "role" για συμβατότητα / ευκολότερο debug (jwt.io)
            new("role", role)
        };

        if (!string.IsNullOrWhiteSpace(fullName))
            claims.Add(new("full_name", fullName));
        if (!string.IsNullOrWhiteSpace(email))
            claims.Add(new(JwtRegisteredClaimNames.Email, email));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: _opt.Issuer,
            audience: _opt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_opt.ExpiresMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}
