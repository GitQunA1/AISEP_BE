namespace AISEP.BLL.DTOs.Responses
{
    public class DealOwnershipAssignmentStatusResponse
    {
        public int DealId { get; set; }
        public int ProjectId { get; set; }
        public string DocumentHash { get; set; } = string.Empty;
        public string InvestorWallet { get; set; } = string.Empty;
        public bool IsOwnerAssignedOnChain { get; set; }
        public string RegisterDocumentTxHash { get; set; } = string.Empty;
        public string TimestampOnBlockchain { get; set; } = string.Empty;
        public List<string> OnChainOwners { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }
}
