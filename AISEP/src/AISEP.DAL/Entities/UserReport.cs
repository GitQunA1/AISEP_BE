using AISEP.DAL.Enums;

namespace AISEP.DAL.Entities
{
    public class UserReport
    {
        public int UserReportId { get; set; }
        public int ReporterId { get; set; }
        public int ReportedUserId { get; set; }
        public UserReportCategory Category { get; set; }
        public string? Reason { get; set; }
        public string? EvidenceUrl { get; set; }
        public string? EvidenceImageUrls { get; set; }
        public string? VideoEvidenceUrl { get; set; }
        public UserReportStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User Reporter { get; set; } = null!;
        public User ReportedUser { get; set; } = null!;
    }
}
