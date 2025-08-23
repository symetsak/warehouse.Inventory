using BlazorClient.Services;
using System.Net.Http.Json;

public class BarcodeService : IBarcodeService
{
    private readonly HttpClient _http;
    public BarcodeService(HttpClient http) => _http = http;

    public async Task<bool> CreateAsync(int productId, string code, string type = "CODE128", bool isPrimary = true)
    {
        var dto = new { ProductId = productId, Code = code, Type = type, IsPrimary = isPrimary };
        var resp = await _http.PostAsJsonAsync("api/productbarcodes", dto);
        return resp.IsSuccessStatusCode;
    }
}
