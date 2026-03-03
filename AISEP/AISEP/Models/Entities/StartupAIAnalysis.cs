namespace AISEP.Models.Entities
{
    public class StartupAIAnalysis
    {
        public int EvaluationId { get; set; }
        public int StartupId { get; set; }
        public int? PotentialScore { get; set; }
        public int? ChaosScore { get; set; }
        public string? AnalysisJson { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Startup Startup { get; set; } = null!;
    }
}
