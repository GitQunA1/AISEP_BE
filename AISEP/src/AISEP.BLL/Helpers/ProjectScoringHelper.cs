using AISEP.DAL.Entities;
using AISEP.DAL.Enums;

namespace AISEP.BLL.Helpers
{
    public class ScorecardBaseScoreResult
    {
        public decimal TeamScore { get; set; }
        public decimal MarketScore { get; set; }
        public decimal ProductScore { get; set; }
        public decimal CompetitionScore { get; set; }
        public decimal TractionScore { get; set; }
        public decimal InvestmentNeedScore { get; set; }
        public decimal TotalScore { get; set; }

        public Dictionary<string, ScoreCriterionBreakdown> ToScoreBreakdown()
        {
            return new Dictionary<string, ScoreCriterionBreakdown>
            {
                ["Team"] = new() { MaxScore = TeamMaxScore, CurrentBaseScore = TeamScore },
                ["Market"] = new() { MaxScore = MarketMaxScore, CurrentBaseScore = MarketScore },
                ["Product"] = new() { MaxScore = ProductMaxScore, CurrentBaseScore = ProductScore },
                ["Competition"] = new() { MaxScore = CompetitionMaxScore, CurrentBaseScore = CompetitionScore },
                ["Traction"] = new() { MaxScore = TractionMaxScore, CurrentBaseScore = TractionScore },
                ["InvestmentNeed"] = new() { MaxScore = InvestmentNeedMaxScore, CurrentBaseScore = InvestmentNeedScore }
            };
        }

        public Dictionary<string, decimal> GetCurrentBaseScores()
        {
            return new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
            {
                ["Team"] = TeamScore,
                ["Market"] = MarketScore,
                ["Product"] = ProductScore,
                ["Competition"] = CompetitionScore,
                ["Traction"] = TractionScore,
                ["InvestmentNeed"] = InvestmentNeedScore
            };
        }

        internal decimal TeamMaxScore { get; set; }
        internal decimal MarketMaxScore { get; set; }
        internal decimal ProductMaxScore { get; set; }
        internal decimal CompetitionMaxScore { get; set; }
        internal decimal TractionMaxScore { get; set; }
        internal decimal InvestmentNeedMaxScore { get; set; }
    }

    public class ScoreCriterionBreakdown
    {
        public decimal MaxScore { get; set; }
        public decimal CurrentBaseScore { get; set; }
    }

    public static class ProjectScoringHelper
    {
        public static decimal CalculateBaseScore(ProjectScorecard scorecard, ScorecardWeightConfig weightConfig)
        {
            return CalculateBaseScoreBreakdown(scorecard, weightConfig).TotalScore;
        }

        public static ScorecardBaseScoreResult CalculateBaseScoreBreakdown(ProjectScorecard scorecard, ScorecardWeightConfig weightConfig)
        {
            var teamScore =
                Average(
                    ConvertThreeLevelEnum(scorecard.TeamSize),
                    ConvertThreeLevelEnum(scorecard.TeamExperience),
                    scorecard.HasTechnicalCofounder ? 1.0m : 0.5m)
                * weightConfig.TeamWeight;

            var marketScore =
                Average(
                    ConvertThreeLevelEnum(scorecard.TargetMarketSize),
                    ConvertThreeLevelEnum(scorecard.MarketGrowth))
                * weightConfig.MarketWeight;

            var productScore =
                Average(
                    ConvertFourLevelEnum(scorecard.ProductReadiness),
                    ConvertThreeLevelEnum(scorecard.IPProtection))
                * weightConfig.ProductWeight;

            var competitionScore = ConvertThreeLevelEnum(scorecard.BarrierToEntry)
                * weightConfig.CompetitionWeight;

            var tractionScore = ConvertFourLevelEnum(scorecard.CurrentTraction)
                * weightConfig.TractionWeight;

            var investmentNeedScore = ConvertThreeLevelEnum(scorecard.RunwayMonths)
                * weightConfig.InvestmentNeedWeight;

            var totalScore = teamScore
                + marketScore
                + productScore
                + competitionScore
                + tractionScore
                + investmentNeedScore;

            return new ScorecardBaseScoreResult
            {
                TeamScore = RoundScore(teamScore),
                MarketScore = RoundScore(marketScore),
                ProductScore = RoundScore(productScore),
                CompetitionScore = RoundScore(competitionScore),
                TractionScore = RoundScore(tractionScore),
                InvestmentNeedScore = RoundScore(investmentNeedScore),
                TotalScore = RoundScore(totalScore),
                TeamMaxScore = RoundScore(weightConfig.TeamWeight),
                MarketMaxScore = RoundScore(weightConfig.MarketWeight),
                ProductMaxScore = RoundScore(weightConfig.ProductWeight),
                CompetitionMaxScore = RoundScore(weightConfig.CompetitionWeight),
                TractionMaxScore = RoundScore(weightConfig.TractionWeight),
                InvestmentNeedMaxScore = RoundScore(weightConfig.InvestmentNeedWeight)
            };
        }

        private static decimal ConvertThreeLevelEnum(TeamSizeEnum value)
        {
            return value switch
            {
                TeamSizeEnum.Solo => 0.5m,
                TeamSizeEnum.TwoFounders => 0.75m,
                TeamSizeEnum.ThreeOrMore => 1.0m,
                _ => 0m
            };
        }

        private static decimal ConvertThreeLevelEnum(TeamExperienceEnum value)
        {
            return value switch
            {
                TeamExperienceEnum.FirstTime => 0.5m,
                TeamExperienceEnum.IndustryExpert => 0.75m,
                TeamExperienceEnum.SerialFounder => 1.0m,
                _ => 0m
            };
        }

        private static decimal ConvertThreeLevelEnum(TargetMarketSizeEnum value)
        {
            return value switch
            {
                TargetMarketSizeEnum.Niche => 0.5m,
                TargetMarketSizeEnum.Medium => 0.75m,
                TargetMarketSizeEnum.Large => 1.0m,
                _ => 0m
            };
        }

        private static decimal ConvertThreeLevelEnum(MarketGrowthEnum value)
        {
            return value switch
            {
                MarketGrowthEnum.Slow => 0.5m,
                MarketGrowthEnum.Steady => 0.75m,
                MarketGrowthEnum.Fast => 1.0m,
                _ => 0m
            };
        }

        private static decimal ConvertThreeLevelEnum(IPProtectionEnum value)
        {
            return value switch
            {
                IPProtectionEnum.None => 0.5m,
                IPProtectionEnum.Defensible => 0.75m,
                IPProtectionEnum.Secured => 1.0m,
                _ => 0m
            };
        }

        private static decimal ConvertThreeLevelEnum(BarrierToEntryEnum value)
        {
            return value switch
            {
                BarrierToEntryEnum.Low => 0.5m,
                BarrierToEntryEnum.Medium => 0.75m,
                BarrierToEntryEnum.High => 1.0m,
                _ => 0m
            };
        }

        private static decimal ConvertThreeLevelEnum(RunwayMonthsEnum value)
        {
            return value switch
            {
                RunwayMonthsEnum.Under6Months => 0.5m,
                RunwayMonthsEnum.SixToTwelveMonths => 0.75m,
                RunwayMonthsEnum.Over12Months => 1.0m,
                _ => 0m
            };
        }

        private static decimal ConvertFourLevelEnum(ProductReadinessEnum value)
        {
            return value switch
            {
                ProductReadinessEnum.Idea => 0.4m,
                ProductReadinessEnum.Prototype => 0.6m,
                ProductReadinessEnum.MVP => 0.8m,
                ProductReadinessEnum.MarketReady => 1.0m,
                _ => 0m
            };
        }

        private static decimal ConvertFourLevelEnum(CurrentTractionEnum value)
        {
            return value switch
            {
                CurrentTractionEnum.PreRevenue => 0.4m,
                CurrentTractionEnum.UserAcquisition => 0.6m,
                CurrentTractionEnum.RevenueGenerating => 0.8m,
                CurrentTractionEnum.ScalingOrProfitable => 1.0m,
                _ => 0m
            };
        }

        private static decimal Average(params decimal[] values)
        {
            return values.Length == 0 ? 0m : values.Sum() / values.Length;
        }

        private static decimal RoundScore(decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }
    }
}
