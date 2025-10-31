using BlazorClient;
using BlazorClient.Services;
using BlazorClient.Services.Announcements;
using BlazorClient.Services.Auth;
using BlazorClient.Services.Connectivity;
// offline-first services
using BlazorClient.Services.Offline;
using BlazorClient.Services.Sync;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http;

namespace BlazorClient
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            // ========================
            // Authorization (Blazor)
            // ========================
            builder.Services.AddAuthorizationCore();

            // ========================
            // Auth services & handler
            // ========================
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<AuthTokenHandler>();

            var apiBase = new Uri("https://localhost:7138"); // API base URL

            // Raw client ΧΩΡΙΣ handler (για refresh κ.λπ.)
            builder.Services.AddHttpClient("API_NOHANDLER", client =>
            {
                client.BaseAddress = apiBase;
            });

            // Κανονικός client ΜΕ handler – όλα τα calls της εφαρμογής
            builder.Services.AddHttpClient("API", client =>
            {
                client.BaseAddress = apiBase;
            })
            .AddHttpMessageHandler<AuthTokenHandler>();

            // Default HttpClient injection => "API"
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

            // ========================
            // Offline / Sync / Connectivity services
            // ========================
            builder.Services.AddScoped<IOfflineDb, OfflineDb>();
            builder.Services.AddScoped<ISyncService, SyncService>();
            builder.Services.AddScoped<OnlineStatusService>();
            builder.Services.AddScoped<PendingCounterService>();
            
            // ========================
            // Domain services DI
            // ========================
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IBarcodeService, BarcodeService>();
            builder.Services.AddScoped<IWarehouseService, WarehouseService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IInventoryService, InventoryService>();
            builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();


            // ========================
            // Build & bootstrap
            // ========================
            var host = builder.Build();

            // Προσπάθησε ανανέωση token στην εκκίνηση
            await host.Services.GetRequiredService<AuthService>().TryRefreshAsync();

            await host.RunAsync();
        }
    }
}
