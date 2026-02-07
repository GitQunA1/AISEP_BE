using AISEP.Models.Enums;

namespace AISEP.Models
{
    public class WithdrawRequest
    {
        public Guid Id { get; set; }

        public Guid WalletId { get; set; }

        public decimal Amount { get; set; }

        public WithdrawRequestStatus Status { get; set; }

        public DateTime RequestedAt { get; set; }

        // Navigation properties
        public Wallet Wallet { get; set; } = null!;
    }
}
