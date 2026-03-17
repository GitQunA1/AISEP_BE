namespace AISEP.BLL.DTOs.Responses
{
    public class TransactionStatusResponse
    {
        public int TransactionId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
