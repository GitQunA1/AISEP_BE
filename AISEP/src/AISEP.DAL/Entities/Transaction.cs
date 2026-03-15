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

        // Reference to what this payment is for
        public string? ReferenceType { get; set; }  // "Subscription" or "Booking"
        public int? ReferenceId { get; set; }        // PackageId or BookingId

        // SePay fields
        public string? PaymentCode { get; set; }
        public string? SepayTransactionId { get; set; }
        public string? PaymentContent { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
    }
}
