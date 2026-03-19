using AISEP.DAL.Entities;

namespace AISEP.BLL.Services.AI
{
    public class ComponentEvaluation
    {
        public double Score { get; set; }
        public List<string> Evidence { get; set; } = [];
        public List<string> MissingData { get; set; } = [];
        public double Confidence { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class GeminiAnalysisResult
    {
        public ComponentEvaluation? Team { get; set; }
        public ComponentEvaluation? Opportunity { get; set; }
        public ComponentEvaluation? Product { get; set; }
        public ComponentEvaluation? Competition { get; set; }
        public ComponentEvaluation? Marketing { get; set; }
        public ComponentEvaluation? Investment { get; set; }
        public ComponentEvaluation? Other { get; set; }

        public double TeamScore        { get; set; }
        public double OpportunityScore { get; set; }
        public double ProductScore     { get; set; }
        public double CompetitionScore { get; set; }
        public double MarketingScore   { get; set; }
        public double InvestmentScore  { get; set; }
        public double OtherScore       { get; set; }
        public int    PotentialScore   { get; set; }
        public int    ChaosScore       { get; set; }
        public bool   IsEligibleStartup  { get; set; }
        public string EligibilityReason  { get; set; } = string.Empty;
        public string Summary            { get; set; } = string.Empty;
        public List<string> Strengths { get; set; } = [];
        public List<string> Weaknesses { get; set; } = [];
        public List<string> Recommendations { get; set; } = [];
        public string InvestmentVerdict { get; set; } = string.Empty;
        public List<string> RiskFlags { get; set; } = [];
        public List<string> DealBreakers { get; set; } = [];
        public List<string> DueDiligenceQuestions { get; set; } = [];
        public string InvestorNextStep { get; set; } = string.Empty;
    }

    public interface IGeminiAiService
    {
        Task<GeminiAnalysisResult> AnalyzeProjectAsync(Project project, IEnumerable<Document> documents);
        Task<GeminiAnalysisResult> AnalyzeProjectForInvestorAsync(Project project, IEnumerable<Document> documents);
    }
}
