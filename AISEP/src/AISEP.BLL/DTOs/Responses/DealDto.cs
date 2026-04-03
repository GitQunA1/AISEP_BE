namespace AISEP.BLL.DTOs.Responses
{
    public class DealDto
    {
        public int DealId { get; set; }
        public int InvestorId { get; set; }
        public string InvestorName { get; set; } = string.Empty;
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string StartupName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool StartupConfirmed { get; set; }
        public bool InvestorConfirmed { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime DealDate { get; set; }
        public string? PaymentMethod { get; set; }
        public decimal? EquityPercentage { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string? ContractPdfUrl { get; set; }
        public DateTime? ContractSignedAt { get; set; }
        public int? ContractSignedByUserId { get; set; }
    }
}
