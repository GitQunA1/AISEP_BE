namespace AISEP.BLL.DTOs.Responses
{
    public class BlockchainVerificationResponse
    {
        public bool IsAuthentic { get; set; }
        public string TxHash { get; set; } = string.Empty;
        public string TimestampOnBlockchain { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
