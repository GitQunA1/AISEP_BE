namespace AISEP.BLL.DTOs.Responses
{
    public class ConnectionRequestDto
    {
        public int ConnectionRequestId { get; set; }
        public int InvestorId { get; set; }
        public int ProjectId { get; set; }
        public int? ChatSessionId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Message { get; set; }
        public DateTime? ResponseDate { get; set; }
    }
}
