using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace BlazorClient.Services.Auth
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _js;
        private readonly CustomAuthStateProvider _authStateProvider;

        // storage keys
        private const string TokenKey = "wi_access_token";
        private const string ExpKey = "wi_access_expires";
        private const string UserKey = "wi_user";
        private const string RefreshKey = "wi_refresh_token";
        private const string RefreshExp = "wi_refresh_expires";

        public AuthService(HttpClient http, IJSRuntime js, AuthenticationStateProvider authStateProvider)
        {
            _http = http;
            _js = js;
            _authStateProvider = (CustomAuthStateProvider)authStateProvider;
        }

        // ===== DTOs matching API =====
        public record LoginRequest(string Username, string Password);
        public record AuthResponse(
            string AccessToken,
            DateTime ExpiresAt,
            string UserName,
            string Role,
            string FullName,
            string RefreshToken,
            DateTime RefreshExpiresAt
        );
        public record RefreshRequest(string RefreshToken);

        // ===== Public shape for current user =====
        public record CurrentUser(int Id, string FullName, string? Role);

        // ===== LOGIN =====
        public async Task<bool> LoginAsync(string username, string password)
        {
            var res = await _http.PostAsJsonAsync("api/Auth/login", new LoginRequest(username, password));
            if (!res.IsSuccessStatusCode) return false;

            var data = await res.Content.ReadFromJsonAsync<AuthResponse>();
            if (data is null || string.IsNullOrWhiteSpace(data.AccessToken)) return false;

            // store access
            await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, data.AccessToken);
            await _js.InvokeVoidAsync("localStorage.setItem", ExpKey, data.ExpiresAt.ToString("o"));
            await _js.InvokeVoidAsync("localStorage.setItem", UserKey, data.UserName);

            // store refresh
            await _js.InvokeVoidAsync("localStorage.setItem", RefreshKey, data.RefreshToken);
            await _js.InvokeVoidAsync("localStorage.setItem", RefreshExp, data.RefreshExpiresAt.ToString("o"));

            // set auth header + notify UI
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", data.AccessToken);
            await _authStateProvider.SetTokenAsync(data.AccessToken);

            return true;
        }

        // ===== LOGOUT =====
        public async Task LogoutAsync()
        {
            try
            {
                var rt = await _js.InvokeAsync<string?>("localStorage.getItem", RefreshKey);
                if (!string.IsNullOrWhiteSpace(rt))
                    await _http.PostAsJsonAsync("api/Auth/logout", new RefreshRequest(rt));
            }
            catch { /* swallow in client logout */ }

            await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
            await _js.InvokeVoidAsync("localStorage.removeItem", ExpKey);
            await _js.InvokeVoidAsync("localStorage.removeItem", UserKey);
            await _js.InvokeVoidAsync("localStorage.removeItem", RefreshKey);
            await _js.InvokeVoidAsync("localStorage.removeItem", RefreshExp);

            _http.DefaultRequestHeaders.Authorization = null;
            await _authStateProvider.SetTokenAsync(null);
        }

        // ===== REFRESH / ATTACH =====
        public async Task<bool> TryRefreshAsync()
        {
            var token = await _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
            var expIso = await _js.InvokeAsync<string?>("localStorage.getItem", ExpKey);
            var rt = await _js.InvokeAsync<string?>("localStorage.getItem", RefreshKey);
            var rtExpIso = await _js.InvokeAsync<string?>("localStorage.getItem", RefreshExp);

            if (!string.IsNullOrWhiteSpace(token) &&
                DateTime.TryParse(expIso, null, System.Globalization.DateTimeStyles.RoundtripKind, out var accessExp) &&
                DateTime.UtcNow < accessExp)
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                await _authStateProvider.SetTokenAsync(token);
                return true;
            }

            if (string.IsNullOrWhiteSpace(rt) ||
                !DateTime.TryParse(rtExpIso, null, System.Globalization.DateTimeStyles.RoundtripKind, out var rtExp) ||
                DateTime.UtcNow >= rtExp)
            {
                await LogoutAsync();
                return false;
            }

            var resp = await _http.PostAsJsonAsync("api/Auth/refresh", new RefreshRequest(rt));
            if (!resp.IsSuccessStatusCode) { await LogoutAsync(); return false; }

            var data = await resp.Content.ReadFromJsonAsync<AuthResponse>();
            if (data is null) { await LogoutAsync(); return false; }

            await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, data.AccessToken);
            await _js.InvokeVoidAsync("localStorage.setItem", ExpKey, data.ExpiresAt.ToString("o"));
            await _js.InvokeVoidAsync("localStorage.setItem", RefreshKey, data.RefreshToken);
            await _js.InvokeVoidAsync("localStorage.setItem", RefreshExp, data.RefreshExpiresAt.ToString("o"));

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", data.AccessToken);
            await _authStateProvider.SetTokenAsync(data.AccessToken);
            return true;
        }

        public async Task TryAttachTokenAsync() => _ = await TryRefreshAsync();

        // ===== CURRENT USER from claims =====
        public async Task<CurrentUser?> GetCurrentUserAsync()
        {
            var state = await _authStateProvider.GetAuthenticationStateAsync();
            var principal = state.User;
            if (principal?.Identity?.IsAuthenticated != true)
                return null;

            var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier) ?? principal.FindFirst("sub");
            var ok = int.TryParse(idClaim?.Value, out var uid);
            if (!ok) return null;

            var fullName =
                principal.FindFirst("full_name")?.Value ??
                principal.FindFirst(ClaimTypes.Name)?.Value ??
                principal.Identity?.Name ?? string.Empty;

            var role =
                principal.FindFirst(ClaimTypes.Role)?.Value ??
                principal.FindFirst("role")?.Value;

            return new CurrentUser(uid, fullName, role);
        }
    }
}
