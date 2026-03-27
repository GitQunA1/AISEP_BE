namespace AISEP.BLL.DTOs.Responses
{
    public class UserReportResponse
    {
        public int UserReportId { get; set; }
        public int ReporterId { get; set; }
        public int ReportedUserId { get; set; }
        public string? Reason { get; set; }
        public string? EvidenceUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
