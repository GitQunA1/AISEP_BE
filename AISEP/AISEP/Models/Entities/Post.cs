namespace AISEP.Models.Entities
{
    public class Post
    {
        public int PostId { get; set; }
        public int ConnectionRequestId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public bool StartupApproved { get; set; }
        public bool InvestorApproved { get; set; }
        public DateTime? PublishedAt { get; set; }

        // Navigation properties
        public ConnectionRequest ConnectionRequest { get; set; } = null!;
    }
}
