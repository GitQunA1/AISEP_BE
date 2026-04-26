using AISEP.DAL.Enums;

namespace AISEP.BLL.DTOs.Responses
{
    public class InvestorResponse
    {
        public int InvestorId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? OrganizationName { get; set; }
        public string? InvestmentTaste { get; set; }
        public string? WalletAddress { get; set; }
        public decimal? InvestmentAmount { get; set; }
        public DateTime? InvestmentDate { get; set; }
        public RiskTolerance? RiskTolerance { get; set; }
        public string? InvestmentRegion { get; set; }
        public List<string> Industries { get; set; } = [];
        public string? PreferredStage { get; set; }
        public string? PreviousInvestments { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
