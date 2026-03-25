using AISEP.BLL.Services.AI;

namespace AISEP.BLL.DTOs.Responses
{
    public class ScoreBreakdownItem
    {
        public string Component { get; set; } = string.Empty;
        public double MaxPoints { get; set; }
        public double Score { get; set; }
    }

    public class StartupAIAnalysisResponse
    {
        public int EvaluationId { get; set; }
        public int ProjectId { get; set; }
        public int? PotentialScore { get; set; }
        public int? ChaosScore { get; set; }
        public string? AnalysisJson { get; set; }
        public GeminiAnalysisResult? Analysis { get; set; }
        public List<ScoreBreakdownItem> ScoreBreakdown { get; set; } = [];
        public bool? IsEligibleStartup { get; set; }
        public string? EligibilityReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
