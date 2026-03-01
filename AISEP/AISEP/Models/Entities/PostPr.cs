namespace AISEP.Models.Entities
{
    public class PostPr
    {
        public int Id { get; set; }
        public int ConnectionId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public bool StartupApproved { get; set; }
        public bool InvestorApproved { get; set; }
        public DateTime? PublishedAt { get; set; }

        // Navigation properties
        public ConnectionRequest ConnectionRequest { get; set; } = null!;
    }
}
