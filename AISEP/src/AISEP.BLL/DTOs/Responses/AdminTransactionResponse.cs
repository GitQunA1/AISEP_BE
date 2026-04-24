namespace AISEP.BLL.DTOs.Responses
{
    public class AdminTransactionResponse
    {
        public int TransactionId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ReferenceType { get; set; }
        public int? ReferenceId { get; set; }
        public string? PaymentCode { get; set; }
        public string? SepayTransactionId { get; set; }
        public string? PaymentContent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
