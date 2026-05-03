using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AISEP.DAL.Enums;

namespace AISEP.DAL.Entities
{
    public class ProjectScorecard
    {
        [Key]
        [ForeignKey("Project")]
        public int ProjectId { get; set; }

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

        public Project Project { get; set; } = null!;
    }
}
