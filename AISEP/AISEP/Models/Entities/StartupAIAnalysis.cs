namespace AISEP.Models.Entities
{
    public class StartupAIAnalysis
    {
        public int EvaluationId { get; set; }
        public int ProjectId { get; set; }
        public int? PotentialScore { get; set; }
        public int? ChaosScore { get; set; }
        public string? AnalysisJson { get; set; }
        public bool? IsEligibleStartup { get; set; }
        public string? EligibilityReason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Project Project { get; set; } = null!;
    }
}
