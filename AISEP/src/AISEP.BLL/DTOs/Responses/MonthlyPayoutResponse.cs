namespace AISEP.BLL.DTOs.Responses
{
    public class MonthlyPayoutResponse
    {
        public int MonthlyPayoutId { get; set; }
        public int WalletId { get; set; }
        public int AdvisorId { get; set; }
        public string AdvisorName { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public int? ApprovedById { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? PaidAt { get; set; }
        public int? PaidById { get; set; }
        public string? PaidByName { get; set; }
        public DateTime? RejectedAt { get; set; }
        public int? RejectedById { get; set; }
        public string? RejectReason { get; set; }
        public string? Note { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountHolderName { get; set; } = string.Empty;
    }
}
