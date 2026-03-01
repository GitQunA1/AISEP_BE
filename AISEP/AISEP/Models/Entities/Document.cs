using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AISEP.Models.Enums;

namespace AISEP.Models.Entities
{
    public class Document
    {
        public int DocumentId { get; set; }
        public int ProjectId { get; set; }
        public DocumentType DocumentType { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string? FileHash { get; set; }
        public string? BlockchainTxHash { get; set; }
        public bool IsIpProtected { get; set; }
        public DateTime? VerifiedAt { get; set; }

        // Navigation properties
        public Project Project { get; set; } = null!;
        public ICollection<BlockchainProof> BlockchainProofs { get; set; } = new List<BlockchainProof>();
    }
}
