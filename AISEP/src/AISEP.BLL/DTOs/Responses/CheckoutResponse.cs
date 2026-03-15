namespace AISEP.BLL.DTOs.Responses
{
    public class CheckoutResponse
    {
        public int TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentCode { get; set; } = string.Empty;
        public string QrCodeUrl { get; set; } = string.Empty;
    }
}
