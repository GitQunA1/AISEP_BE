using AISEP.DAL.Enums;

namespace AISEP.BLL.DTOs.Requests
{
    public class UpdateProjectRequest
    {
        public string?    ProjectName            { get; set; }
        public IFormFile? ProjectImageFile       { get; set; }
        public string?    ShortDescription       { get; set; }
        public int?       StageOptionId          { get; set; }
        public string?    ProblemStatement       { get; set; }
        public string?    SolutionDescription    { get; set; }
        public string?    TargetCustomers        { get; set; }
        public string?    UniqueValueProposition { get; set; }
        public string?    BusinessModel          { get; set; }
        public string?    Competitors            { get; set; }
        public int?       IndustryOptionId       { get; set; }
        public TeamSizeEnum? TeamSize { get; set; }
        public TeamExperienceEnum? TeamExperience { get; set; }
        public bool? HasTechnicalCofounder { get; set; }
        public TargetMarketSizeEnum? TargetMarketSize { get; set; }
        public MarketGrowthEnum? MarketGrowth { get; set; }
        public ProductReadinessEnum? ProductReadiness { get; set; }
        public IPProtectionEnum? IPProtection { get; set; }
        public BarrierToEntryEnum? BarrierToEntry { get; set; }
        public CurrentTractionEnum? CurrentTraction { get; set; }
        public RunwayMonthsEnum? RunwayMonths { get; set; }
    }
}
