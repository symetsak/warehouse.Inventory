using BlazorClient;
using BlazorClient.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace BlazoeClient
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            // HttpClient για WebApi (πρέπει να τρέχει το API σε ξεχωριστό port)
            builder.Services.AddScoped(sp =>
                new HttpClient { BaseAddress = new Uri("https://localhost:7138") }   // webapi base URL
);

            // Services DI
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IWarehouseService, WarehouseService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IInventoryService, InventoryService>();

            await builder.Build().RunAsync();
        }
    }
}
