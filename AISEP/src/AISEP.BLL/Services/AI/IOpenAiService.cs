using AISEP.DAL.Entities;
using System.Text.Json.Serialization;

namespace AISEP.BLL.Services.AI
{
    public class AiAnalysisResult
    {
        public int AIAdjustmentScore { get; set; }
        public string Reasoning { get; set; } = string.Empty;
        public List<string> Strengths { get; set; } = [];
        public List<string> Weaknesses { get; set; } = [];
        public List<string> Advice { get; set; } = [];
        public decimal BaseScore { get; set; }
        public decimal FinalPotentialScore { get; set; }
        public bool   IsEligibleStartup  { get; set; }
        public string EligibilityReason  { get; set; } = string.Empty;
    }

    public class AiEligibilityResult
    {
        [JsonPropertyName("is_eligible_startup")]
        public bool IsEligibleStartup { get; set; }

        [JsonPropertyName("eligibility_reason")]
        public string EligibilityReason { get; set; } = string.Empty;
    }

    public interface IOpenAiService
    {
        Task<AiAnalysisResult> AnalyzeProjectAsync(Project project, decimal baseScore);
        Task<AiEligibilityResult> EvaluateStartupEligibilityAsync(Project project, IEnumerable<Document> documents);
        Task<AiAnalysisResult> AnalyzeProjectForInvestorAsync(Project project, decimal baseScore);
    }
}
