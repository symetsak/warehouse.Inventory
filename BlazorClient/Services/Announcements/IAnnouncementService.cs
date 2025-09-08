using Shared.DTOs;

namespace BlazorClient.Services.Announcements
{
    public interface IAnnouncementService
    {
        Task<List<AnnouncementDto>> GetAllAsync();
        Task CreateAsync(AnnouncementDto dto);
        Task DeleteAsync(int id);
        Task TogglePinAsync(int id, bool pin);
    }
}
