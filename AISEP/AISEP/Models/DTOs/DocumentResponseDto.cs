using AISEP.Models.Enums;

namespace AISEP.Models.DTOs
{
    /// <summary>
    /// Response DTO trả về thông tin Document cho client.
    /// </summary>
    public class DocumentResponseDto
    {
        public int Id { get; set; }
        public int StartupId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string? FileHash { get; set; }
        public string? BlockchainTxHash { get; set; }
        public bool IsIpProtected { get; set; }
        public DateTime? VerifiedAt { get; set; }
    }
}
