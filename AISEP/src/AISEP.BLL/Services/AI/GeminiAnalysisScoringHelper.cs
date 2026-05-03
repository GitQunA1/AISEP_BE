using System.Text.Json;
using System.Linq;
using AISEP.BLL.DTOs.Responses;
using AISEP.DAL.Entities;

namespace AISEP.BLL.Services.AI
{
    public static class GeminiAnalysisScoringHelper
    {
        public static int CalculatePotentialScore(GeminiAnalysisResult result)
        {
            static double Normalize(double score) => NormalizeScore(score);
            var maxPoints = GetMaxPointProfile();

            var totalPoints =
                (Normalize(GetComponentScore(result.Team, result.TeamScore)) / 10.0) * maxPoints.Team +
                (Normalize(GetComponentScore(result.Opportunity, result.OpportunityScore)) / 10.0) * maxPoints.Opportunity +
                (Normalize(GetComponentScore(result.Product, result.ProductScore)) / 10.0) * maxPoints.Product +
                (Normalize(GetComponentScore(result.Competition, result.CompetitionScore)) / 10.0) * maxPoints.Competition +
                (Normalize(GetComponentScore(result.Marketing, result.MarketingScore)) / 10.0) * maxPoints.Marketing +
                (Normalize(GetComponentScore(result.Investment, result.InvestmentScore)) / 10.0) * maxPoints.Investment +
                (Normalize(GetComponentScore(result.Other, result.OtherScore)) / 10.0) * maxPoints.Other;

            return (int)Math.Round(totalPoints, MidpointRounding.AwayFromZero);
        }

        public static int ApplyDataQualitySanityCap(int potentialScore, GeminiAnalysisResult result, Project project)
        {
            var capped = potentialScore;
            var weakCoreFields = CountWeakCoreFields(project);

            // Additional cap when core project fields are placeholder-like.
            if (weakCoreFields >= 4)
            {
                capped = Math.Min(capped, 30);
            }
            else if (weakCoreFields >= 2)
            {
                capped = Math.Min(capped, 45);
            }

            return capped;
        }

        public static void NormalizeAnalysisResult(GeminiAnalysisResult result, bool includeInvestorFields)
        {
            static double ClampScore(double value) => NormalizeScore(value);
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

                // Guardrail: score high but evidence missing -> reduce to a cautious level.
                if (component.Score > 6.0 && component.Evidence.Count == 0)
                {
                    component.Score = 4.5;
                    component.Reason = string.IsNullOrWhiteSpace(component.Reason)
                        ? "Điểm đã được điều chỉnh do thiếu bằng chứng cụ thể."
                        : component.Reason + " | Đã điều chỉnh do thiếu bằng chứng cụ thể.";
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

            static double Normalize(double score) => NormalizeScore(score);
            double Score(ComponentEvaluation? component, double fallback) => Normalize(GetComponentScore(component, fallback));
            var maxPoints = GetMaxPointProfile();

            var team = Score(analysis.Team, analysis.TeamScore);
            var opportunity = Score(analysis.Opportunity, analysis.OpportunityScore);
            var product = Score(analysis.Product, analysis.ProductScore);
            var competition = Score(analysis.Competition, analysis.CompetitionScore);
            var marketing = Score(analysis.Marketing, analysis.MarketingScore);
            var investment = Score(analysis.Investment, analysis.InvestmentScore);
            var other = Score(analysis.Other, analysis.OtherScore);

            return
            [
                new ScoreBreakdownItem
                {
                    ComponentKey = "Team",
                    Component = ToVietnameseComponentName("Team"),
                    MaxPoints = maxPoints.Team,
                    Score = Math.Round((team / 10.0) * maxPoints.Team, 2)
                },
                new ScoreBreakdownItem
                {
                    ComponentKey = "Opportunity",
                    Component = ToVietnameseComponentName("Opportunity"),
                    MaxPoints = maxPoints.Opportunity,
                    Score = Math.Round((opportunity / 10.0) * maxPoints.Opportunity, 2)
                },
                new ScoreBreakdownItem
                {
                    ComponentKey = "Product",
                    Component = ToVietnameseComponentName("Product"),
                    MaxPoints = maxPoints.Product,
                    Score = Math.Round((product / 10.0) * maxPoints.Product, 2)
                },
                new ScoreBreakdownItem
                {
                    ComponentKey = "Competition",
                    Component = ToVietnameseComponentName("Competition"),
                    MaxPoints = maxPoints.Competition,
                    Score = Math.Round((competition / 10.0) * maxPoints.Competition, 2)
                },
                new ScoreBreakdownItem
                {
                    ComponentKey = "Marketing",
                    Component = ToVietnameseComponentName("Marketing"),
                    MaxPoints = maxPoints.Marketing,
                    Score = Math.Round((marketing / 10.0) * maxPoints.Marketing, 2)
                },
                new ScoreBreakdownItem
                {
                    ComponentKey = "Investment",
                    Component = ToVietnameseComponentName("Investment"),
                    MaxPoints = maxPoints.Investment,
                    Score = Math.Round((investment / 10.0) * maxPoints.Investment, 2)
                },
                new ScoreBreakdownItem
                {
                    ComponentKey = "Other",
                    Component = ToVietnameseComponentName("Other"),
                    MaxPoints = maxPoints.Other,
                    Score = Math.Round((other / 10.0) * maxPoints.Other, 2)
                }
            ];
        }

        private static string ToVietnameseComponentName(string componentKey)
        {
            return componentKey switch
            {
                "Team" => "Đội ngũ",
                "Opportunity" => "Cơ hội thị trường",
                "Product" => "Sản phẩm",
                "Competition" => "Cạnh tranh",
                "Marketing" => "Marketing & bán hàng",
                "Investment" => "Nhu cầu đầu tư",
                "Other" => "Khác",
                _ => componentKey
            };
        }

        private static (double Team, double Opportunity, double Product, double Competition, double Marketing, double Investment, double Other)
            GetMaxPointProfile()
        {
            return (20, 30, 35, 5, 5, 3, 2);
        }

        private static double GetComponentScore(ComponentEvaluation? component, double fallbackScore)
        {
            return component?.Score > 0 ? component.Score : fallbackScore;
        }

        private static int CountWeakCoreFields(Project project)
        {
            var core = new[]
            {
                project.ProjectName,
                project.ShortDescription,
                project.ProblemStatement,
                project.SolutionDescription,
                project.TargetCustomers
            };

            return core.Count(IsWeakFieldValue);
        }

        private static bool IsWeakFieldValue(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            var normalized = value.Trim().ToLowerInvariant();
            if (normalized.Length < 8)
            {
                return true;
            }

            return normalized is "string" or "null" or "n/a" or "na" or "none" or "undefined" or "test";
        }

        private static double NormalizeScore(double value)
        {
            return Math.Clamp(value, 0.0, 10.0);
        }
    }
}
