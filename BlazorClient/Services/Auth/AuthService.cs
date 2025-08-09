using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.JSInterop;

namespace BlazorClient.Services.Auth
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _js;
        private readonly CustomAuthStateProvider _authStateProvider;

        private const string TokenKey = "wi_access_token";
        private const string ExpKey = "wi_access_expires";
        private const string UserKey = "wi_user";

        public AuthService(HttpClient http, IJSRuntime js, AuthenticationStateProvider authStateProvider)
        {
            _http = http;
            _js = js;
            _authStateProvider = (CustomAuthStateProvider)authStateProvider;
        }

        public record LoginRequest(string Username, string Password);
        public record AuthResponse(string AccessToken, DateTime ExpiresAt, string UserName, string Role, string FullName);

        public async Task<bool> LoginAsync(string username, string password)
        {
            var res = await _http.PostAsJsonAsync("api/Auth/login", new LoginRequest(username, password));
            if (!res.IsSuccessStatusCode) return false;

            var data = await res.Content.ReadFromJsonAsync<AuthResponse>();
            if (data is null || string.IsNullOrWhiteSpace(data.AccessToken)) return false;

            await _js.InvokeVoidAsync("localStorage.setItem", ExpKey, data.ExpiresAt.ToString("o"));
            await _js.InvokeVoidAsync("localStorage.setItem", UserKey, data.UserName);

            // αποθήκευση + ενημέρωση auth state
            await _authStateProvider.SetTokenAsync(data.AccessToken);

            // βάλε header για HttpClient
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", data.AccessToken);
            return true;
        }

        public async Task LogoutAsync()
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", ExpKey);
            await _js.InvokeVoidAsync("localStorage.removeItem", UserKey);
            await _authStateProvider.SetTokenAsync(null);
            _http.DefaultRequestHeaders.Authorization = null;
        }

        public async Task TryAttachTokenAsync()
        {
            var token = await _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
            var expIso = await _js.InvokeAsync<string?>("localStorage.getItem", ExpKey);
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(expIso)) return;

            if (DateTime.TryParse(expIso, null, System.Globalization.DateTimeStyles.RoundtripKind, out var expUtc)
                && DateTime.UtcNow < expUtc)
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                // ενημέρωσε και το auth state (σε περίπτωση hard refresh)
                await _authStateProvider.SetTokenAsync(token);
            }
            else
            {
                await LogoutAsync();
            }
        }
    }
}
