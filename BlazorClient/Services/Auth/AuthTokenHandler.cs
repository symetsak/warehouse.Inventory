using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace BlazorClient.Services.Auth
{
    public class AuthTokenHandler : DelegatingHandler
    {
        private readonly IJSRuntime _js;
        private readonly IHttpClientFactory _httpFactory;
        private readonly CustomAuthStateProvider _authStateProvider;

        private static readonly SemaphoreSlim _refreshLock = new(1, 1);

        private const string TokenKey = "wi_access_token";
        private const string ExpKey = "wi_access_expires";
        private const string RefreshKey = "wi_refresh_token";
        private const string RefreshExp = "wi_refresh_expires";

        // DTO same as API
        private record AuthResponse(
            string AccessToken,
            DateTime ExpiresAt,
            string UserName,
            string Role,
            string FullName,
            string RefreshToken,
            DateTime RefreshExpiresAt
        );

        public AuthTokenHandler(
            IJSRuntime js,
            IHttpClientFactory httpFactory,
            AuthenticationStateProvider authStateProvider)
        {
            _js = js;
            _httpFactory = httpFactory;
            _authStateProvider = (CustomAuthStateProvider)authStateProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Authorization αν έχουμε έγκυρο access
            var token = await _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
            var expIso = await _js.InvokeAsync<string?>("localStorage.getItem", ExpKey);

            if (!string.IsNullOrWhiteSpace(token) &&
                DateTime.TryParse(expIso, null, System.Globalization.DateTimeStyles.RoundtripKind, out var expUtc))
            {
                var remaining = expUtc - DateTime.UtcNow;
                if (remaining.TotalSeconds <= 60)
                {
                    var refreshed = await TryRefreshAsync(cancellationToken);
                    if (refreshed)
                        token = await _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
                }

                if (!string.IsNullOrWhiteSpace(token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Εκτέλεση
            var response = await base.SendAsync(request, cancellationToken);

            // Αν 401 → ΜΙΑ φορά refresh + retry
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshed = await TryRefreshAsync(cancellationToken);
                if (refreshed)
                {
                    response.Dispose();
                    var newToken = await _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
                    if (!string.IsNullOrWhiteSpace(newToken))
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);

                    response = await base.SendAsync(request, cancellationToken);
                }
            }

            return response;
        }

        private async Task<bool> TryRefreshAsync(CancellationToken ct)
        {
            await _refreshLock.WaitAsync(ct);
            try
            {
                var rt = await _js.InvokeAsync<string?>("localStorage.getItem", RefreshKey);
                var rtExpIso = await _js.InvokeAsync<string?>("localStorage.getItem", RefreshExp);

                if (string.IsNullOrWhiteSpace(rt) ||
                    !DateTime.TryParse(rtExpIso, null, System.Globalization.DateTimeStyles.RoundtripKind, out var rtExp) ||
                    DateTime.UtcNow >= rtExp)
                {
                    await ClearAllAsync();
                    return false;
                }

                // χρησιμοποιούμε τον "API_NOHANDLER" για να ΜΗΝ περάσει από τον ίδιο handler
                var raw = _httpFactory.CreateClient("API_NOHANDLER");
                var resp = await raw.PostAsJsonAsync("api/Auth/refresh", new { RefreshToken = rt }, ct);
                if (!resp.IsSuccessStatusCode)
                {
                    await ClearAllAsync();
                    return false;
                }

                var data = await resp.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: ct);
                if (data is null)
                {
                    await ClearAllAsync();
                    return false;
                }

                // αποθήκευση νέων
                await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, data.AccessToken);
                await _js.InvokeVoidAsync("localStorage.setItem", ExpKey, data.ExpiresAt.ToString("o"));
                await _js.InvokeVoidAsync("localStorage.setItem", RefreshKey, data.RefreshToken);
                await _js.InvokeVoidAsync("localStorage.setItem", RefreshExp, data.RefreshExpiresAt.ToString("o"));

                // ενημέρωσε το auth state ώστε να δει το UI τον νέο principal
                await _authStateProvider.SetTokenAsync(data.AccessToken);
                return true;
            }
            catch
            {
                await ClearAllAsync();
                return false;
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        private async Task ClearAllAsync()
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
            await _js.InvokeVoidAsync("localStorage.removeItem", ExpKey);
            await _js.InvokeVoidAsync("localStorage.removeItem", RefreshKey);
            await _js.InvokeVoidAsync("localStorage.removeItem", RefreshExp);
            await _authStateProvider.SetTokenAsync(null);
        }
    }
}
