namespace AISEP.BLL.DTOs.Responses
{
    public class BookingPaymentStatusResponse
    {
        public int BookingId { get; set; }
        public string BookingStatus { get; set; } = string.Empty;
        public bool IsPaid { get; set; }
        public int? TransactionId { get; set; }
        public string? TransactionStatus { get; set; }
        public string? PaymentCode { get; set; }
        public decimal Amount { get; set; }
    }
}
