using AISEP.DAL.Enums;

namespace AISEP.DAL.Entities
{
    public class WithdrawRequest
    {
        public int WithdrawRequestId { get; set; }
        public int WalletId { get; set; }
        public decimal Amount { get; set; }
        public string? BankName { get; set; }
        public string? BankAccount { get; set; }
        public string? ProofImageUrl { get; set; }
        public WithdrawRequestStatus Status { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Wallet Wallet { get; set; } = null!;
    }
}
