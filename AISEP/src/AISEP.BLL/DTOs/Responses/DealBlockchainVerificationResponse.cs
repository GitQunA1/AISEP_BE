namespace AISEP.BLL.DTOs.Responses
{
    public class DealBlockchainVerificationResponse
    {
        public int DealId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string DocumentHash { get; set; } = string.Empty;
        public string InvestorWallet { get; set; } = string.Empty;
        public int StartupId { get; set; }
        public string TimestampOnBlockchain { get; set; } = string.Empty;
        public IReadOnlyList<string> Owners { get; set; } = Array.Empty<string>();
        public bool IsVerified { get; set; }
    }
}
