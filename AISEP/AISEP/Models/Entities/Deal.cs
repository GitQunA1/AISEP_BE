using AISEP.Models.Enums;

namespace AISEP.Models.Entities
{
    public class Deal
    {
        public int DealId { get; set; }
        public int InvestorId { get; set; }
        public int ProjectId { get; set; }
        public decimal Amount { get; set; }
        public bool StartupConfirmed { get; set; }
        public bool InvestorConfirmed { get; set; }
        public DealStatus Status { get; set; }
        public DateTime DealDate { get; set; }
        public string? PaymentMethod { get; set; }
        public decimal? EquityPercentage { get; set; }
        public string? TransactionHash { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletionDate { get; set; }

        // Navigation properties
        public Investor Investor { get; set; } = null!;
        public Project Project { get; set; } = null!;
        public NFTRecord? NFTRecord { get; set; }
    }
}
