using AISEP.BLL.Helpers;
using AISEP.DAL.Entities;
using System.Text.Json.Serialization;

namespace AISEP.BLL.Services.AI
{
    public class AiAnalysisResult
    {
        public decimal TotalAIAdjustmentScore { get; set; }
        public decimal AIAdjustmentScore { get; set; }
        public string Reasoning { get; set; } = string.Empty;
        public List<AiAuditedItem> AuditedItems { get; set; } = [];
        public List<string> Strengths { get; set; } = [];
        public List<string> Weaknesses { get; set; } = [];
        public List<string> Advice { get; set; } = [];
        public decimal BaseScore { get; set; }
        public decimal FinalPotentialScore { get; set; }
        public bool IsEligibleStartup { get; set; }
        public string EligibilityReason { get; set; } = string.Empty;
    }

    public class AiAuditedItem
    {
        public string Criteria { get; set; } = string.Empty;
        public string Finding { get; set; } = string.Empty;
        public decimal Adjustment { get; set; }
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
        Task<AiAnalysisResult> AnalyzeProjectAsync(Project project, ScorecardBaseScoreResult baseScore, string? documentText = null);
        Task<AiEligibilityResult> EvaluateStartupEligibilityAsync(Project project, IEnumerable<Document> documents);
        Task<AiAnalysisResult> AnalyzeProjectForInvestorAsync(Project project, ScorecardBaseScoreResult baseScore, string? documentText = null);
    }
}
