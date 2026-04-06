namespace AISEP.BLL.DTOs.Responses
{
    public class WithdrawRequestResponse
    {
        public int WithdrawRequestId { get; set; }
        public int WalletId { get; set; }
        public int AdvisorId { get; set; }
        public string AdvisorName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? BankName { get; set; }
        public string? BankAccount { get; set; }
        public string? ProofImageUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public int? ApprovedById { get; set; }
        public DateTime? RejectedAt { get; set; }
        public int? RejectedById { get; set; }
        public string? RejectionReason { get; set; }
    }
}
