namespace AISEP.BLL.DTOs.Responses
{
    public class ConnectionRequestDto
    {
        public int ConnectionRequestId { get; set; }
        public int InvestorId { get; set; }
        public string InvestorName { get; set; } = string.Empty;
        public int ProjectId { get; set; }
        public string StartupName { get; set; } = string.Empty;
        public int? ChatSessionId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Message { get; set; }
        public DateTime? ResponseDate { get; set; }
    }
}
