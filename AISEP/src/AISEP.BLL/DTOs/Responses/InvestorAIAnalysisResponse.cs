using AISEP.BLL.Services.AI;

namespace AISEP.BLL.DTOs.Responses
{
    public class InvestorAIAnalysisResponse
    {
        public int AnalysisId { get; set; }
        public int InvestorId { get; set; }
        public int ProjectId { get; set; }
        public string? AnalysisJson { get; set; }
        public GeminiAnalysisResult? Analysis { get; set; }
        public int? PotentialScore { get; set; }
        public int? ChaosScore { get; set; }
        public List<ScoreBreakdownItem> ScoreBreakdown { get; set; } = [];
        public string InvestmentVerdict { get; set; } = string.Empty;
        public List<string> RiskFlags { get; set; } = [];
        public List<string> DealBreakers { get; set; } = [];
        public List<string> DueDiligenceQuestions { get; set; } = [];
        public string InvestorNextStep { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
