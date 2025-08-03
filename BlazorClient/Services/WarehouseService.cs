using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Shared.DTOs;

namespace BlazorClient.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly HttpClient _http;
        public WarehouseService(HttpClient http) => _http = http;

        public async Task<IEnumerable<WarehouseDto>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<IEnumerable<WarehouseDto>>("api/Warehouses")
                   ?? Enumerable.Empty<WarehouseDto>();
        }

        public async Task<WarehouseDto?> GetByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<WarehouseDto>($"api/Warehouses/{id}");
        }

        public async Task<WarehouseDto> CreateAsync(CreateWarehouseDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/Warehouses", dto);
            response.EnsureSuccessStatusCode();
            var warehouse = await response.Content.ReadFromJsonAsync<WarehouseDto>();
            if (warehouse is null)
                throw new InvalidOperationException("API returned empty body for Create");
            return warehouse;
        }

        public async Task UpdateAsync(int id, CreateWarehouseDto dto)
        {
            var response = await _http.PutAsJsonAsync($"api/Warehouses/{id}", dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/Warehouses/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}