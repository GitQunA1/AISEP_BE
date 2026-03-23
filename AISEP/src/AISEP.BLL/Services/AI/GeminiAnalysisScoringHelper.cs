using System.Text.Json;
using AISEP.BLL.DTOs.Responses;

namespace AISEP.BLL.Services.AI
{
    public static class GeminiAnalysisScoringHelper
    {
        public static int CalculatePotentialScore(GeminiAnalysisResult result)
        {
            static double Normalize(double score) => Math.Clamp(score, 0.0, 2.0);

            var weighted =
                0.30 * Normalize(GetComponentScore(result.Team, result.TeamScore)) +
                0.25 * Normalize(GetComponentScore(result.Opportunity, result.OpportunityScore)) +
                0.15 * Normalize(GetComponentScore(result.Product, result.ProductScore)) +
                0.10 * Normalize(GetComponentScore(result.Competition, result.CompetitionScore)) +
                0.10 * Normalize(GetComponentScore(result.Marketing, result.MarketingScore)) +
                0.05 * Normalize(GetComponentScore(result.Investment, result.InvestmentScore)) +
                0.05 * Normalize(GetComponentScore(result.Other, result.OtherScore));

            // Map weighted average from [0..2] to [0..100].
            return (int)Math.Round((weighted / 2.0) * 100.0, MidpointRounding.AwayFromZero);
        }

        public static void NormalizeAnalysisResult(GeminiAnalysisResult result, bool includeInvestorFields)
        {
            static double ClampScore(double value) => Math.Clamp(value, 0.0, 2.0);
            static double ClampConfidence(double value) => Math.Clamp(value, 0.0, 1.0);

            void NormalizeComponent(ComponentEvaluation? component, Action<double> setLegacyScore)
            {
                if (component is null)
                {
                    return;
                }

                component.Score = ClampScore(component.Score);
                component.Confidence = ClampConfidence(component.Confidence);
                component.Evidence ??= [];
                component.MissingData ??= [];
                component.Reason ??= string.Empty;

                // Guardrail: score cao nhưng thiếu bằng chứng => giảm về mức thận trọng.
                if (component.Score > 1.0 && component.Evidence.Count == 0)
                {
                    component.Score = 0.9;
                    component.Reason = string.IsNullOrWhiteSpace(component.Reason)
                    ? "Điểm đã được điều chỉnh do thiếu bằng chứng cụ thể."
                    : component.Reason + " | Adjusted: thiếu bằng chứng cụ thể.";
                }

                setLegacyScore(component.Score);
            }

            NormalizeComponent(result.Team, s => result.TeamScore = s);
            NormalizeComponent(result.Opportunity, s => result.OpportunityScore = s);
            NormalizeComponent(result.Product, s => result.ProductScore = s);
            NormalizeComponent(result.Competition, s => result.CompetitionScore = s);
            NormalizeComponent(result.Marketing, s => result.MarketingScore = s);
            NormalizeComponent(result.Investment, s => result.InvestmentScore = s);
            NormalizeComponent(result.Other, s => result.OtherScore = s);

            result.TeamScore = ClampScore(result.TeamScore);
            result.OpportunityScore = ClampScore(result.OpportunityScore);
            result.ProductScore = ClampScore(result.ProductScore);
            result.CompetitionScore = ClampScore(result.CompetitionScore);
            result.MarketingScore = ClampScore(result.MarketingScore);
            result.InvestmentScore = ClampScore(result.InvestmentScore);
            result.OtherScore = ClampScore(result.OtherScore);
            result.ChaosScore = Math.Clamp(result.ChaosScore, 0, 100);

            result.Strengths ??= [];
            result.Weaknesses ??= [];
            result.Recommendations ??= [];
            result.Summary ??= string.Empty;

            if (includeInvestorFields)
            {
                result.RiskFlags ??= [];
                result.DealBreakers ??= [];
                result.DueDiligenceQuestions ??= [];
                result.InvestmentVerdict ??= string.Empty;
                result.InvestorNextStep ??= string.Empty;
            }
        }

        public static GeminiAnalysisResult? DeserializeAnalysisJson(string? analysisJson)
        {
            if (string.IsNullOrWhiteSpace(analysisJson))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<GeminiAnalysisResult>(
                    analysisJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                return null;
            }
        }

        public static List<ScoreBreakdownItem> BuildBreakdown(GeminiAnalysisResult? analysis)
        {
            if (analysis is null)
            {
                return [];
            }

            double Normalize(double score) => Math.Clamp(score, 0.0, 2.0);
            double Score(ComponentEvaluation? component, double fallback) => Normalize(GetComponentScore(component, fallback));

            var team = Score(analysis.Team, analysis.TeamScore);
            var opportunity = Score(analysis.Opportunity, analysis.OpportunityScore);
            var product = Score(analysis.Product, analysis.ProductScore);
            var competition = Score(analysis.Competition, analysis.CompetitionScore);
            var marketing = Score(analysis.Marketing, analysis.MarketingScore);
            var investment = Score(analysis.Investment, analysis.InvestmentScore);
            var other = Score(analysis.Other, analysis.OtherScore);

            return
            [
                new ScoreBreakdownItem { Component = "Team", Weight = 0.30, Score = team, WeightedContribution = Math.Round(0.30 * (team / 2.0) * 100.0, 2) },
                new ScoreBreakdownItem { Component = "Opportunity", Weight = 0.25, Score = opportunity, WeightedContribution = Math.Round(0.25 * (opportunity / 2.0) * 100.0, 2) },
                new ScoreBreakdownItem { Component = "Product", Weight = 0.15, Score = product, WeightedContribution = Math.Round(0.15 * (product / 2.0) * 100.0, 2) },
                new ScoreBreakdownItem { Component = "Competition", Weight = 0.10, Score = competition, WeightedContribution = Math.Round(0.10 * (competition / 2.0) * 100.0, 2) },
                new ScoreBreakdownItem { Component = "Marketing", Weight = 0.10, Score = marketing, WeightedContribution = Math.Round(0.10 * (marketing / 2.0) * 100.0, 2) },
                new ScoreBreakdownItem { Component = "Investment", Weight = 0.05, Score = investment, WeightedContribution = Math.Round(0.05 * (investment / 2.0) * 100.0, 2) },
                new ScoreBreakdownItem { Component = "Other", Weight = 0.05, Score = other, WeightedContribution = Math.Round(0.05 * (other / 2.0) * 100.0, 2) }
            ];
        }

        private static double GetComponentScore(ComponentEvaluation? component, double fallbackScore)
        {
            return component?.Score > 0 ? component.Score : fallbackScore;
        }
    }
}
