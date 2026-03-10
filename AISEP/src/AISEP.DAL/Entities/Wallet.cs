using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISEP.DAL.Entities
{
    public class Wallet
    {
        public int WalletId { get; set; }

        public int AdvisorId { get; set; }

        public decimal Balance { get; set; }

        public string Currency { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        // Navigation properties
        public Advisor Advisor { get; set; } = null!;

        public ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
        public ICollection<WithdrawRequest> WithdrawRequests { get; set; } = new List<WithdrawRequest>();
    }
}
