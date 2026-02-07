using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AISEP.Models.Enums;

namespace AISEP.Models
{
    public class WalletTransaction
    {
        public Guid Id { get; set; }

        public Guid WalletId { get; set; }

        public decimal Amount { get; set; }

        public WalletTransactionType Type { get; set; }

        public WalletTransactionStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Wallet Wallet { get; set; } = null!;
    }
}
