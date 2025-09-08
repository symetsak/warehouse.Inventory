namespace Core.Entities
{
    public class Announcement
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string PublisherFullName { get; set; } = string.Empty;
        public bool IsPinned { get; set; } = false;        
        public DateTime? PinnedAt { get; set; }
    }

}
