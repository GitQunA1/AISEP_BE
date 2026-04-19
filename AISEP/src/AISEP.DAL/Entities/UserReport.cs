using AISEP.DAL.Enums;

namespace AISEP.DAL.Entities
{
    public class UserReport
    {
        public int UserReportId { get; set; }
        public int? BookingId { get; set; }
        public int ReporterId { get; set; }
        public int? ResolvedById { get; set; }
        public UserReportCategory Category { get; set; }
        public string? Reason { get; set; }
        public string? EvidenceUrl { get; set; }
        public string? EvidenceImageUrls { get; set; }
        public string? VideoEvidenceUrl { get; set; }
        public UserReportStatus Status { get; set; }
        public string? ResolutionNote { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Booking? Booking { get; set; }
        public User Reporter { get; set; } = null!;
        public User? ResolvedBy { get; set; }
    }
}
