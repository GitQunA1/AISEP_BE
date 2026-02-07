using System.ComponentModel.DataAnnotations.Schema;
using AISEP.Models.Enums;

namespace AISEP.Models
{
    public class BlockchainProof
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public string TransactionHash { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public VerificationStatus VerificationStatus { get; set; }

        // Navigation properties
        public Document Document { get; set; } = null!;
    }
}
