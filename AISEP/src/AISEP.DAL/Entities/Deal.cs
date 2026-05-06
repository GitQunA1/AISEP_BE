using AISEP.DAL.Enums;

namespace AISEP.DAL.Entities
{
    public class Deal
    {
        public int DealId { get; set; }
        public int InvestorId { get; set; }
        public int ProjectId { get; set; }
        public decimal InvestedAmount { get; set; }
        public InvestmentType Type { get; set; }
        public decimal? EquityPercentage { get; set; }
        public string? ExchangeTerms { get; set; }
        public bool StartupConfirmed { get; set; }
        public bool InvestorConfirmed { get; set; }
        public DealStatus Status { get; set; }
        public DateTime DealDate { get; set; }
        public string? DocumentUrl { get; set; }
        public string? DocumentHash { get; set; }
        public string? BlockchainTxHash { get; set; }
        public DateTime? BlockchainVerifiedAt { get; set; }
        public string? BlockchainErrorMessage { get; set; }
        public UserRole InitiatorRole { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletionDate { get; set; }

        public Investor Investor { get; set; } = null!;
        public Project Project { get; set; } = null!;
    }
}
