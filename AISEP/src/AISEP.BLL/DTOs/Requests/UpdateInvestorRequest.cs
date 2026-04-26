using AISEP.DAL.Enums;

namespace AISEP.BLL.DTOs.Requests
{
    public class UpdateInvestorRequest
    {
        public string?         OrganizationName    { get; set; }
        public string?         InvestmentTaste     { get; set; }
        public string?         WalletAddress       { get; set; }
        public decimal?        InvestmentAmount    { get; set; }
        public DateTime?       InvestmentDate      { get; set; }
        public RiskTolerance?  RiskTolerance       { get; set; }
        public string?         InvestmentRegion    { get; set; }
        public List<int>?      IndustryOptionIds   { get; set; }
        public PreferredStage? PreferredStage      { get; set; }
        public string?         PreviousInvestments { get; set; }
        public IFormFile?      ProfileImageFile    { get; set; }
    }
}
