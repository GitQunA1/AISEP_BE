using AISEP.DAL.Enums;

namespace AISEP.DAL.Entities
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public TransactionStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

       
        public string? ReferenceType { get; set; }  
        public int? ReferenceId { get; set; }        

        // SePay fields
        public string? PaymentCode { get; set; }
        public string? SepayTransactionId { get; set; }
        public string? PaymentContent { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
    }
}
