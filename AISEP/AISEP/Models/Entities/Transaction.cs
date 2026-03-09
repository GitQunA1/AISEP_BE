using AISEP.Models.Enums;

namespace AISEP.Models.Entities
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public TransactionStatus Status { get; set; }
        public string? PayosOrderCode { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User User { get; set; } = null!;
    }
}
