using AISEP.BLL.Helpers;

namespace AISEP.BLL.Services.AI
{
    public static class AiAuditAdjustmentGuard
    {
        private const decimal MaxTotalBonus = 5m;

        public static void Normalize(AiAnalysisResult result, ScorecardBaseScoreResult baseScore)
        {
            result.AuditedItems ??= [];

            if (result.AuditedItems.Count == 0)
            {
                result.TotalAIAdjustmentScore = Round(Math.Clamp(
                    result.TotalAIAdjustmentScore,
                    -baseScore.TotalScore,
                    MaxTotalBonus));
                result.AIAdjustmentScore = result.TotalAIAdjustmentScore;
                return;
            }

            var currentScores = baseScore.GetCurrentBaseScores();
            foreach (var item in result.AuditedItems)
            {
                var criterion = ResolveCriterion(item.Criteria);
                if (criterion is null || !currentScores.TryGetValue(criterion, out var currentBaseScore))
                {
                    item.Adjustment = 0m;
                    continue;
                }

                item.Criteria = criterion;
                item.Adjustment = Round(Math.Clamp(item.Adjustment, -currentBaseScore, MaxTotalBonus));
            }

            CapTotalBonus(result.AuditedItems);

            result.TotalAIAdjustmentScore = Round(result.AuditedItems.Sum(x => x.Adjustment));
            result.TotalAIAdjustmentScore = Math.Clamp(result.TotalAIAdjustmentScore, -baseScore.TotalScore, MaxTotalBonus);
            result.AIAdjustmentScore = result.TotalAIAdjustmentScore;
        }

        private static void CapTotalBonus(List<AiAuditedItem> auditedItems)
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
