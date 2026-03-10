using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AISEP.DAL.Enums;

namespace AISEP.DAL.Entities
{
    public class WalletTransaction
    {
        public int WalletTransactionId { get; set; }

        public int WalletId { get; set; }

        public decimal Amount { get; set; }

        public WalletTransactionType Type { get; set; }

        public WalletTransactionStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Wallet Wallet { get; set; } = null!;
    }
}
