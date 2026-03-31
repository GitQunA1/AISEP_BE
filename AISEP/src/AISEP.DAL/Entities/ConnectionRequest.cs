using AISEP.DAL.Enums;

namespace AISEP.DAL.Entities
{
    public class ConnectionRequest
    {
        public int ConnectionRequestId { get; set; }
        public int InvestorId { get; set; }
        public int ProjectId { get; set; }
        public ConnectionRequestStatus Status { get; set; }
        public string? Message { get; set; }
        public DateTime? ResponseDate { get; set; }

        // Navigation properties
        public Investor Investor { get; set; } = null!;
        public Project Project { get; set; } = null!;
        public ChatSession? ChatSession { get; set; }
        public ICollection<PostPr> PostPrs { get; set; } = new List<PostPr>();
    }
}
