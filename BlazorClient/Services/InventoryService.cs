using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Shared.DTOs;

namespace BlazorClient.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly HttpClient _http;
        public InventoryService(HttpClient http) => _http = http;

        public async Task<IEnumerable<InventoryDto>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<IEnumerable<InventoryDto>>("api/Inventory")
                   ?? Enumerable.Empty<InventoryDto>();
        }

        public async Task<InventoryDto?> GetByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<InventoryDto>($"api/Inventory/{id}");
        }

        public async Task<InventoryDto> CreateAsync(CreateInventoryDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/Inventory", dto);
            response.EnsureSuccessStatusCode();
            var inventory = await response.Content.ReadFromJsonAsync<InventoryDto>();
            if (inventory is null)
                throw new InvalidOperationException("API returned empty body for Create");
            return inventory;
        }

        public async Task UpdateAsync(int id, CreateInventoryDto dto)
        {
            var response = await _http.PutAsJsonAsync($"api/Inventory/{id}", dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/Inventory/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<DashboardStatsDto?> GetDashboardStatsAsync()
        {
            return await _http.GetFromJsonAsync<DashboardStatsDto>("api/dashboard/stats");
        }

        public async Task<List<WarehouseStatsDto>> GetWarehouseStatsAsync()
        {
            return await _http.GetFromJsonAsync<List<WarehouseStatsDto>>("api/dashboard/warehouse-stats")
                   ?? new List<WarehouseStatsDto>();
        }

        public async Task<TodayIODto> GetTodayIOAsync()
        {
            return await _http.GetFromJsonAsync<TodayIODto>("api/dashboard/today-io")
                ?? new TodayIODto();
        }

    }
}

