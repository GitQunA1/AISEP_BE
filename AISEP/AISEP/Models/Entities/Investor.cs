using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AISEP.Models.Enums;

namespace AISEP.Models.Entities
{
    public class Investor
    {
      public int InvestorId { get; set; }

      public int UserId { get; set; }

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
      public ICollection<InvestorAIAnalysis> InvestorAIAnalyses { get; set; } = new List<InvestorAIAnalysis>();
    }
}
