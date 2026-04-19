namespace AISEP.BLL.DTOs.Responses
{
    public class UserReportResponse
    {
        public int UserReportId { get; set; }
        public int? BookingId { get; set; }
        public int ReporterId { get; set; }
        public int? ResolvedById { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<string> EvidenceImageUrls { get; set; } = [];
        public string? VideoEvidenceUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ResolutionNote { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
