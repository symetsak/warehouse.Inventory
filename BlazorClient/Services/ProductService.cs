using System.Net.Http.Json;
using Shared.DTOs;

namespace BlazorClient.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _http;
        public ProductService(HttpClient http) => _http = http;

        public async Task<IEnumerable<ProductDto>> GetAllAsync() =>
            await _http.GetFromJsonAsync<IEnumerable<ProductDto>>("api/Products")
            ?? Enumerable.Empty<ProductDto>();

        public async Task<ProductDto?> GetByIdAsync(int id) =>
            await _http.GetFromJsonAsync<ProductDto>($"api/Products/{id}");

        public async Task<ProductDto> CreateAsync(CreateProductDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/Products", dto);
            response.EnsureSuccessStatusCode();
            var product = await response.Content.ReadFromJsonAsync<ProductDto>();
            if (product is null)
                throw new InvalidOperationException("API returned empty body for Create");
            return product;
        }

        public async Task UpdateAsync(int id, CreateProductDto dto)
        {
            var response = await _http.PutAsJsonAsync($"api/Products/{id}", dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/Products/{id}");
            response.EnsureSuccessStatusCode();
        }
        public async Task<ProductDto?> GetByBarcodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            var resp = await _http.GetAsync($"api/products/by-barcode/{Uri.EscapeDataString(code)}");
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<ProductDto>();
        }

    }
}