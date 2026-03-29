using AISEP.DAL.Enums;
using Sieve.Attributes;

namespace AISEP.DAL.Entities
{
    public class Deal
    {
        public int DealId { get; set; }
        public int InvestorId { get; set; }
        public int ProjectId { get; set; }
        [Sieve(CanFilter = true, CanSort = true)]
        public decimal Amount { get; set; }
        public bool StartupConfirmed { get; set; }
        public bool InvestorConfirmed { get; set; }
        [Sieve(CanFilter = true, CanSort = true)]
        public DealStatus Status { get; set; }
        [Sieve(CanFilter = true, CanSort = true)]
        public DateTime DealDate { get; set; }
        public string? PaymentMethod { get; set; }
        public decimal? EquityPercentage { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string? ContractPdfUrl { get; set; }
        public DateTime? ContractSignedAt { get; set; }
        public int? ContractSignedByUserId { get; set; }

        // Navigation properties
        public Investor Investor { get; set; } = null!;
        public Project Project { get; set; } = null!;
        public NFTRecord? NFTRecord { get; set; }
    }
}
