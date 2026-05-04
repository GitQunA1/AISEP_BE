using AISEP.BLL.Services.AI;

namespace AISEP.BLL.DTOs.Responses
{
    public class StartupAIAnalysisResponse
    {
        public int EvaluationId { get; set; }
        public int ProjectId { get; set; }
        public decimal? BaseScore { get; set; }
        public decimal? AIAdjustmentScore { get; set; }
        public decimal? FinalPotentialScore { get; set; }
        public string? AnalysisJson { get; set; }
        public AIAnalysisReportDto? Analysis { get; set; }
        public bool? IsEligibleStartup { get; set; }
        public string? EligibilityReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
