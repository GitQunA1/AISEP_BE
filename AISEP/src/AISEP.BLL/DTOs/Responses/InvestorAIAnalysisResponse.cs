using AISEP.BLL.Services.AI;

namespace AISEP.BLL.DTOs.Responses
{
    public class InvestorAIAnalysisResponse
    {
        public int AnalysisId { get; set; }
        public int InvestorId { get; set; }
        public int ProjectId { get; set; }
        public string? AnalysisJson { get; set; }
        public AiAnalysisResult? Analysis { get; set; }
        public decimal? BaseScore { get; set; }
        public decimal? AIAdjustmentScore { get; set; }
        public decimal? FinalPotentialScore { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
