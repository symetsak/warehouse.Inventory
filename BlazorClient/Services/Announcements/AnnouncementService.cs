using Shared.DTOs;
using System.Net.Http.Json;

namespace BlazorClient.Services.Announcements
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly HttpClient _http;
        public AnnouncementService(HttpClient http) => _http = http;

        public Task<List<AnnouncementDto>?> RawGetAllAsync()
            => _http.GetFromJsonAsync<List<AnnouncementDto>>("api/announcements");

        public async Task<List<AnnouncementDto>> GetAllAsync()
            => await RawGetAllAsync() ?? new();

        public async Task CreateAsync(AnnouncementDto dto)
            => (await _http.PostAsJsonAsync("api/announcements", dto)).EnsureSuccessStatusCode();

        public async Task DeleteAsync(int id)
            => (await _http.DeleteAsync($"api/announcements/{id}")).EnsureSuccessStatusCode();

        public async Task TogglePinAsync(int id, bool pin)
        {
            var url = pin ? $"api/announcements/{id}/pin" : $"api/announcements/{id}/unpin";
            (await _http.PatchAsync(url, null)).EnsureSuccessStatusCode();
        }


    }
}
