using Microsoft.AspNetCore.Components.Authorization;
using BlazorClient;
using BlazorClient.Services;
using BlazorClient.Services.Auth;
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

            builder.Services.AddAuthorizationCore();

            // ========================
            // Register Auth & Handler
            // ========================
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<AuthTokenHandler>();

            var apiBase = new Uri("https://localhost:7138"); // βάλε του API σου

            // 1) "Raw" client ΧΩΡΙΣ handler – θα τον χρησιμοποιεί ΜΟΝΟ ο handler για /auth/refresh
            builder.Services.AddHttpClient("API_NOHANDLER", client =>
            {
                client.BaseAddress = apiBase;
            });

            // 2) Κανονικός client ΜΕ handler – όλα τα calls της εφαρμογής
            builder.Services.AddHttpClient("API", client =>
            {
                client.BaseAddress = apiBase;
            })
            .AddHttpMessageHandler<AuthTokenHandler>();

            // Χρησιμοποιούμε το ίδιο για injection χωρίς όνομα
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

            // ========================
            // Services DI
            // ========================
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IBarcodeService, BarcodeService>();
            builder.Services.AddScoped<IWarehouseService, WarehouseService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IInventoryService, InventoryService>();

            // ========================
            // Εκκίνηση host
            // ========================
            var host = builder.Build();

            // Προσπάθησε να επισυνάψεις / ανανεώσεις το token στην εκκίνηση
            await host.Services.GetRequiredService<AuthService>().TryRefreshAsync();

            await host.RunAsync();
        }
    }
}

