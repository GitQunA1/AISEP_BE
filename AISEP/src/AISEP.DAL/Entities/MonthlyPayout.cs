using AISEP.DAL.Enums;

namespace AISEP.DAL.Entities
{
    public class MonthlyPayout
    {
        public int MonthlyPayoutId { get; set; }
        public int WalletId { get; set; }
        public int AdvisorId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Amount { get; set; }
        public MonthlyPayoutStatus Status { get; set; } = MonthlyPayoutStatus.Pending;
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public int? PaidById { get; set; }
        public string? Note { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountHolderName { get; set; } = string.Empty;

        public Wallet Wallet { get; set; } = null!;
        public Advisor Advisor { get; set; } = null!;
        public User? PaidBy { get; set; }
        public ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
    }
}
