namespace AISEP.BLL.DTOs.Requests
{
    public class CreateUserReportRequest
    {
        public int ReportedUserId { get; set; }
        public string? Reason { get; set; }
        public string? EvidenceUrl { get; set; }
    }
}
