using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;

namespace AISEP.BLL.Services.AI
{
    public static class AIAnalysisReportBuilder
    {
        private const decimal MaxTotalBonus = 5m;

        private static readonly string[] CriteriaOrder =
        [
            "Team",
            "Market",
            "Product",
            "Competition",
            "Traction",
            "InvestmentNeed"
        ];

        public static AIAnalysisReportDto Build(AiAnalysisResult aiResult, ScorecardBaseScoreResult baseScore)
        {
            aiResult.AuditedItems ??= [];
            aiResult.Strengths ??= [];
            aiResult.Weaknesses ??= [];
            aiResult.Advice ??= [];

            var scoreBreakdown = baseScore.ToScoreBreakdown();
            var aiItemsByCriteria = aiResult.AuditedItems
                .Select(item => new { Criterion = ResolveCriterion(item.Criteria), Item = item })
                .Where(x => x.Criterion is not null)
                .GroupBy(x => x.Criterion!)
                .ToDictionary(
                    group => group.Key,
                    group => new
                    {
                        Finding = string.Join(" ", group
                            .Select(x => x.Item.Finding?.Trim())
                            .Where(finding => !string.IsNullOrWhiteSpace(finding))),
                        Adjustment = group.Sum(x => x.Item.Adjustment)
                    },
                    StringComparer.OrdinalIgnoreCase);

            var auditedItems = new List<AuditedItemDto>();

            foreach (var criterion in CriteriaOrder)
            {
                var criterionScore = scoreBreakdown[criterion];
                aiItemsByCriteria.TryGetValue(criterion, out var aiItem);

                var adjustment = aiItem is null
                    ? 0m
                    : Round(Math.Clamp(aiItem.Adjustment, -criterionScore.CurrentBaseScore, MaxTotalBonus));

                auditedItems.Add(new AuditedItemDto
                {
                    Criteria = criterion,
                    MaxScore = criterionScore.MaxScore,
                    BaseScore = criterionScore.CurrentBaseScore,
                    Finding = aiItem?.Finding ?? string.Empty,
                    Adjustment = adjustment,
                    FinalScore = Round(Math.Max(0m, criterionScore.CurrentBaseScore + adjustment))
                });
            }

            CapTotalBonus(auditedItems);
            RecalculateFinalScores(auditedItems);

            var totalAdjustment = Round(auditedItems.Sum(x => x.Adjustment));
            var totalFinalScore = Round(auditedItems.Sum(x => x.FinalScore));

            return new AIAnalysisReportDto
            {
                TotalBaseScore = baseScore.TotalScore,
                TotalAIAdjustmentScore = totalAdjustment,
                TotalFinalScore = Math.Clamp(totalFinalScore, 0m, 100m),
                AuditedItems = auditedItems,
                Strengths = aiResult.Strengths,
                Weaknesses = aiResult.Weaknesses,
                Advice = aiResult.Advice
            };
        }

        private static void CapTotalBonus(List<AuditedItemDto> auditedItems)
        {
            var total = auditedItems.Sum(x => x.Adjustment);
            if (total <= MaxTotalBonus)
            {
                return;
            }

            var excess = total - MaxTotalBonus;
            foreach (var item in auditedItems.Where(x => x.Adjustment > 0).Reverse())
            {
                if (excess <= 0)
                {
                    break;
                }

                var reduction = Math.Min(item.Adjustment, excess);
                item.Adjustment = Round(item.Adjustment - reduction);
                excess -= reduction;
            }
        }

        private static void RecalculateFinalScores(List<AuditedItemDto> auditedItems)
        {
            foreach (var item in auditedItems)
            {
                item.FinalScore = Round(Math.Max(0m, item.BaseScore + item.Adjustment));
            }
        }

        private static string? ResolveCriterion(string? criteria)
        {
            if (string.IsNullOrWhiteSpace(criteria))
            {
                return null;
            }

            var normalized = criteria.Trim().ToLowerInvariant();
            if (normalized.Contains("team") || normalized.Contains("đội") || normalized.Contains("nhân sự"))
            {
                return "Team";
            }

            if (normalized.Contains("market") || normalized.Contains("thị trường"))
            {
                return "Market";
            }

            if (normalized.Contains("product") || normalized.Contains("sản phẩm"))
            {
                return "Product";
            }

            if (normalized.Contains("competition") || normalized.Contains("cạnh tranh"))
            {
                return "Competition";
            }

            if (normalized.Contains("traction") || normalized.Contains("lực kéo") || normalized.Contains("doanh thu"))
            {
                return "Traction";
            }

            if (normalized.Contains("investment") || normalized.Contains("runway") || normalized.Contains("vốn"))
            {
                return "InvestmentNeed";
            }

            return null;
        }

        private static decimal Round(decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }
    }
}
