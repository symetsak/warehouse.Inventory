using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.DTOs;

namespace BlazorClient.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<UserDto?> GetByIdAsync(int id);
        Task<UserDto> CreateAsync(CreateUserDto dto);
        Task UpdateAsync(int id, CreateUserDto dto);
        Task DeleteAsync(int id);
    }
}