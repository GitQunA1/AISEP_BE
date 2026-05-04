namespace AISEP.BLL.DTOs.Requests
{
    public class UpdateScorecardWeightRequest
    {
        public decimal TeamWeight { get; set; }
        public decimal MarketWeight { get; set; }
        public decimal ProductWeight { get; set; }
        public decimal CompetitionWeight { get; set; }
        public decimal TractionWeight { get; set; }
        public decimal InvestmentNeedWeight { get; set; }
    }
}
