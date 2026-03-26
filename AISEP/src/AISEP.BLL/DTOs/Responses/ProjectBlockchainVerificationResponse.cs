namespace AISEP.BLL.DTOs.Responses
{
    public class ProjectBlockchainVerificationResponse
    {
        public bool IsFullyVerified { get; set; }
        public int TotalDocuments { get; set; }
        public int VerifiedDocuments { get; set; }
        public List<int> UnverifiedDocumentIds { get; set; } = new();
        public List<VerifiedProjectDocumentDto> VerifiedDocumentDetails { get; set; } = new();
    }

    public class VerifiedProjectDocumentDto
    {
        public int DocumentId { get; set; }
        public string TxHash { get; set; } = string.Empty;
        public string TimestampOnBlockchain { get; set; } = string.Empty;
        public string SignerAddress { get; set; } = string.Empty;
    }
}
