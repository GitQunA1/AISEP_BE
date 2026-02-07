using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AISEP.Models.Enums;

namespace AISEP.Models
{
    public class Investor
    {
      public Guid Id { get; set; }

      public Guid UserId { get; set; }

      public string? OrganizationName { get; set; }

      public string? InvestmentTaste { get; set; }

      public string? WalletAddress { get; set; }

      public decimal? InvestmentAmount { get; set; }

      public DateTime? InvestmentDate { get; set; }

      public RiskTolerance? RiskTolerance { get; set; } // Low, Medium, High

      public string? InvestmentRegion { get; set; }

      public string? FocusIndustry { get; set; }

      public PreferredStage? PreferredStage { get; set; } // Idea, MVP, Growth, Scale

      public string? PreviousInvestments { get; set; }

        // Navigation properties
      public User User { get; set; } = null!;

      public ICollection<ConnectionRequest> ConnectionRequests { get; set; } = new List<ConnectionRequest>();
      public ICollection<Deal> Deals { get; set; } = new List<Deal>();
    }
}
