using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISEP.Models
{
    public class Wallet
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public decimal Balance { get; set; }

        public string Currency { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;

        public ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
        public ICollection<WithdrawRequest> WithdrawRequests { get; set; } = new List<WithdrawRequest>();
    }
}
