using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Shared.DTOs;

namespace BlazorClient.Services
{
    public class UserService : IUserService
    {
        private readonly HttpClient _http;
        public UserService(HttpClient http) => _http = http;

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<IEnumerable<UserDto>>("api/Users")
                   ?? Enumerable.Empty<UserDto>();
        }

        public async Task<UserDto?> GetByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<UserDto>($"api/Users/{id}");
        }

        public async Task<UserDto> CreateAsync(CreateUserDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/Users", dto);
            response.EnsureSuccessStatusCode();
            var user = await response.Content.ReadFromJsonAsync<UserDto>();
            if (user is null)
                throw new InvalidOperationException("API returned empty body for Create");
            return user;
        }

        public async Task UpdateAsync(int id, CreateUserDto dto)
        {
            var response = await _http.PutAsJsonAsync($"api/Users/{id}", dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/Users/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}