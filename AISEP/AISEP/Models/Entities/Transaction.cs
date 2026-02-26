using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AISEP.Models.Enums;

namespace AISEP.Models.Entities
{
    public class Transaction
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public decimal Amount { get; set; }

        public TransactionType Type { get; set; }

        public TransactionStatus Status { get; set; }

        public DateTime TransactionDate { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
    }
}
