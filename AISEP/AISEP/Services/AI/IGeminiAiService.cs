using AISEP.Models.Entities;

namespace AISEP.Services.AI
{
    public class GeminiAnalysisResult
    {
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
    }

    public interface IGeminiAiService
    {
        Task<GeminiAnalysisResult> AnalyzeProjectAsync(Project project, IEnumerable<Document> documents);
    }
}
