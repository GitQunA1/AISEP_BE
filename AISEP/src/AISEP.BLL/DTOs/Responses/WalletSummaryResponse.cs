namespace AISEP.BLL.DTOs.Responses
{
    public class WalletSummaryResponse
    {
        public int WalletId { get; set; }
        public int AdvisorId { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public decimal PendingWithdrawAmount { get; set; }
        public decimal AvailableBalance { get; set; }
    }
}
