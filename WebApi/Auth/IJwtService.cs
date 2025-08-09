namespace WebApi.Auth;

public interface IJwtService
{
    string CreateToken(int userId, string username, string role, string? fullName = null, string? email = null);
}
