using System.Security.Cryptography;

namespace WebApi.Auth;
public static class TokenGenerator
{
    // base64url χωρίς '=', '+', '/'
    public static string Create(int bytes = 32)
    {
        var buffer = RandomNumberGenerator.GetBytes(bytes);
        var b64 = Convert.ToBase64String(buffer);
        return b64.Replace("+", "-").Replace("/", "_").Replace("=", "");
    }
}
