using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.DTOs;

namespace BlazorClient.Services
{
    public interface IInventoryService
    {
        Task<IEnumerable<InventoryDto>> GetAllAsync();
        Task<InventoryDto?> GetByIdAsync(int id);
        Task<InventoryDto> CreateAsync(CreateInventoryDto dto);
        Task UpdateAsync(int id, CreateInventoryDto dto);
        Task DeleteAsync(int id);
        Task<DashboardStatsDto?> GetDashboardStatsAsync();
        Task<List<WarehouseStatsDto>> GetWarehouseStatsAsync();
        Task<TodayIODto> GetTodayIOAsync();
    }
}

