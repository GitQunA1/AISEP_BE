using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AISEP.Models.Enums;

namespace AISEP.Models.Entities
{
    public class NFTRecord
    {
        public int Id { get; set; }

      public int DealId { get; set; }

        public string TokenId { get; set; } = string.Empty;

        public string TxHash { get; set; } = string.Empty;

        public string OwnerWallet { get; set; } = string.Empty;

        public DateTime MintedAt { get; set; }

        public ValidityStatus ValidityStatus { get; set; }

        public bool Transferable { get; set; }

        public string? PreviousOwnerWallet { get; set; }

        // Navigation properties
        public Deal Deal { get; set; } = null!;
    }
}
