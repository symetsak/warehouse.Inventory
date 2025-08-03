using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.DTOs;

namespace BlazorClient.Services
{
    public interface IWarehouseService
    {
        Task<IEnumerable<WarehouseDto>> GetAllAsync();
        Task<WarehouseDto?> GetByIdAsync(int id);
        Task<WarehouseDto> CreateAsync(CreateWarehouseDto dto);
        Task UpdateAsync(int id, CreateWarehouseDto dto);
        Task DeleteAsync(int id);
    }
}
