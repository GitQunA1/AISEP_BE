using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISEP.Models.Entities
{
    public class Deal
    {
  public int DealId { get; set; }
   public int InvestorId { get; set; }
        public int StartupId { get; set; }
  public decimal Amount { get; set; }
        public bool StartupConfirmed { get; set; }
        public bool InvestorConfirmed { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime DealDate { get; set; }
 public string? PaymentMethod { get; set; }
      public decimal? EquityPercentage { get; set; }
        public string? TransactionHash { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletionDate { get; set; }

     // Navigation properties
  public Investor Investor { get; set; } = null!;
   public Startup Startup { get; set; } = null!;
  public ICollection<NFTRecord> NFTRecords { get; set; } = new List<NFTRecord>();
  }
}
