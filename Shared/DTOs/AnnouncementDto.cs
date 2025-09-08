namespace Shared.DTOs
{
    public class AnnouncementDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string PublisherFullName { get; set; } = string.Empty;
        public bool IsPinned { get; set; }                
        public DateTime? PinnedAt { get; set; }
    }
}
