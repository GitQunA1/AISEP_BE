namespace AISEP.DAL.Entities
{
    public class StartupAIAnalysis
    {
        public int EvaluationId { get; set; }
        public int ProjectId { get; set; }
        public decimal? BaseScore { get; set; }
        public decimal? AIAdjustmentScore { get; set; }
        public decimal? FinalPotentialScore { get; set; }
        public string? AnalysisJson { get; set; }
        public bool? IsEligibleStartup { get; set; }
        public string? EligibilityReason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Project Project { get; set; } = null!;
    }
}
