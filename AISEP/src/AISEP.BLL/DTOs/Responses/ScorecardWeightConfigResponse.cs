namespace AISEP.BLL.DTOs.Responses
{
    public class ScorecardWeightConfigResponse
    {
        public int Id { get; set; }
        public string ConfigName { get; set; } = string.Empty;
        public decimal TeamWeight { get; set; }
        public decimal MarketWeight { get; set; }
        public decimal ProductWeight { get; set; }
        public decimal CompetitionWeight { get; set; }
        public decimal TractionWeight { get; set; }
        public decimal InvestmentNeedWeight { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
