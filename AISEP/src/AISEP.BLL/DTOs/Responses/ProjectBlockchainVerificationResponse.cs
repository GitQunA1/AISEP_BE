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
}
