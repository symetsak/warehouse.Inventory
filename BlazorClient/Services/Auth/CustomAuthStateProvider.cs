using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;


namespace BlazorClient.Services.Auth
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _js;
        private const string TokenKey = "wi_access_token";
        private ClaimsPrincipal _user = new(new ClaimsIdentity()); // anonymous

        public CustomAuthStateProvider(IJSRuntime js) => _js = js;

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Διάβασε token από localStorage
            var token = await _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
            if (!string.IsNullOrWhiteSpace(token))
            {
                _user = CreatePrincipalFromJwt(token);
            }
            else
            {
                _user = new ClaimsPrincipal(new ClaimsIdentity());
            }

            return new AuthenticationState(_user);
        }

        public async Task SetTokenAsync(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
                _user = new ClaimsPrincipal(new ClaimsIdentity());
            }
            else
            {
                await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
                _user = CreatePrincipalFromJwt(token);
            }

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_user)));
        }

        private static ClaimsPrincipal CreatePrincipalFromJwt(string jwt)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);

            // ClaimsIdentity με auth type "jwt"
            var identity = new ClaimsIdentity(token.Claims, "jwt");
            return new ClaimsPrincipal(identity);
        }
    }
}