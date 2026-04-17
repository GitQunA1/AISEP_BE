using AISEP.DAL.Enums;

namespace AISEP.DAL.Entities
{
    public class Payout
    {
        public int PayoutId { get; set; }
        public int? PayoutGroupId { get; set; }
        public int WalletId { get; set; }
        public DateTime PeriodFromDate { get; set; }
        public DateTime PeriodToDate { get; set; }
        public decimal Amount { get; set; }
        public MonthlyPayoutStatus Status { get; set; } = MonthlyPayoutStatus.Pending;
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public int? PaidById { get; set; }
        public DateTime? RejectedAt { get; set; }
        public int? RejectedById { get; set; }
        public string? RejectReason { get; set; }
        public DateTime? RetryRequestedAt { get; set; }
        public string? RetryRequestNote { get; set; }
        public string? Note { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountHolderName { get; set; } = string.Empty;

        public Wallet Wallet { get; set; } = null!;
        public PayoutGroup? PayoutGroup { get; set; }
        public User? PaidBy { get; set; }
        public User? RejectedBy { get; set; }
        public ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
    }
}


