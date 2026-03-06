using AISEP.Models.Enums;

namespace AISEP.DTOs.Responses
{
    public class DocumentResponse
    {
        public int DocumentId { get; set; }
        public int ProjectId { get; set; }
        public DocumentType DocumentType { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string? FileUrl { get; set; }
        public string? FileHash { get; set; }
        public string? BlockchainTxHash { get; set; }
        public bool IsIpProtected { get; set; }
        public DateTime? VerifiedAt { get; set; }
    }
}
