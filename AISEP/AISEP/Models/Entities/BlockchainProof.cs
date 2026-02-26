using System.ComponentModel.DataAnnotations.Schema;
using AISEP.Models.Enums;

namespace AISEP.Models.Entities
{
    public class BlockchainProof
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public string TransactionHash { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public VerificationStatus VerificationStatus { get; set; }

        // Navigation properties
        public Document Document { get; set; } = null!;
    }
}
