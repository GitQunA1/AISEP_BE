using AISEP.DAL.Enums;

namespace AISEP.BLL.DTOs.Responses
{
    public class ProjectScorecardDto
    {
        public TeamSizeEnum TeamSize { get; set; }
        public TeamExperienceEnum TeamExperience { get; set; }
        public bool HasTechnicalCofounder { get; set; }
        public TargetMarketSizeEnum TargetMarketSize { get; set; }
        public MarketGrowthEnum MarketGrowth { get; set; }
        public ProductReadinessEnum ProductReadiness { get; set; }
        public IPProtectionEnum IPProtection { get; set; }
        public BarrierToEntryEnum BarrierToEntry { get; set; }
        public CurrentTractionEnum CurrentTraction { get; set; }
        public RunwayMonthsEnum RunwayMonths { get; set; }
        public decimal? CalculatedScore { get; set; }
    }
}
