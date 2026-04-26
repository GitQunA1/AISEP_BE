using AISEP.DAL.Enums;

namespace AISEP.DAL.Entities
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
        public RiskTolerance? RiskTolerance { get; set; }
        public string? InvestmentRegion { get; set; }
        public PreferredStage? PreferredStage { get; set; }
        public string? PreviousInvestments { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? IdentityDocumentUrl { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Unverified;
        public int? ApprovedById { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public int? RejectedById { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? RejectionReason { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public ICollection<ConnectionRequest> ConnectionRequests { get; set; } = new List<ConnectionRequest>();
        public ICollection<Deal> Deals { get; set; } = new List<Deal>();
        public ICollection<InvestorAIAnalysis> InvestorAIAnalyses { get; set; } = new List<InvestorAIAnalysis>();
        public ICollection<InvestorIndustry> InvestorIndustries { get; set; } = new List<InvestorIndustry>();
    }
}
