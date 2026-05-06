namespace AISEP.BLL.DTOs.Responses
{
    public class DealDto
    {
        public int DealId { get; set; }
        public int InvestorId { get; set; }
        public string InvestorName { get; set; } = string.Empty;
        public int ProjectId { get; set; }
        public int StartupId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string StartupName { get; set; } = string.Empty;
        public decimal InvestedAmount { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal? EquityPercentage { get; set; }
        public string? ExchangeTerms { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime DealDate { get; set; }
        public string? DocumentUrl { get; set; }
        public string? DocumentHash { get; set; }
        public string? BlockchainTxHash { get; set; }
        public DateTime? BlockchainVerifiedAt { get; set; }
        public string? BlockchainErrorMessage { get; set; }
        public string InitiatorRole { get; set; } = string.Empty;
        public bool StartupConfirmed { get; set; }
        public bool InvestorConfirmed { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletionDate { get; set; }
    }
}
