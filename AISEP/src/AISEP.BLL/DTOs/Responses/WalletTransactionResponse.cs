namespace AISEP.BLL.DTOs.Responses
{
    public class WalletTransactionResponse
    {
        public int WalletTransactionId { get; set; }
        public int WalletId { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int? PayoutId { get; set; }
    }
}

