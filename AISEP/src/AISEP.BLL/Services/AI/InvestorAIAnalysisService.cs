using System.Text.Json;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Services.Users;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AutoMapper;

namespace AISEP.BLL.Services.AI
{
    public class InvestorAIAnalysisService : IInvestorAIAnalysisService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IGeminiAiService _geminiAiService;
        private readonly IMapper _mapper;

        public InvestorAIAnalysisService(
            IUnitOfWork unitOfWork,
            IUserService userService,
            IGeminiAiService geminiAiService,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _geminiAiService = geminiAiService;
            _mapper = mapper;
        }

        public async Task<InvestorAIAnalysisResponse> AnalyzeProjectForInvestorAsync(int projectId)
        {
            var userId = _userService.GetUserId();
            var investor = await _unitOfWork.Investors.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Investor profile not found.");

            var project = await _unitOfWork.Projects.GetByIdAsync(projectId)
                ?? throw new KeyNotFoundException($"Project {projectId} not found.");
            if (project.Status != ProjectStatus.Approved)
            {
                throw new InvalidOperationException("Investor AI analysis is only available when project status is Approved.");
            }

            var documents = (await _unitOfWork.Documents.GetByProjectIdAsync(projectId)).ToList();
            var result = await _geminiAiService.AnalyzeProjectForInvestorAsync(project, documents);
            NormalizeAnalysisResult(result);
            result.PotentialScore = CalculatePotentialScore(result, project.DevelopmentStage);

            var analysisJson = JsonSerializer.Serialize(result);
            var existing = await _unitOfWork.InvestorAIAnalyses
                .GetByInvestorAndProjectAsync(investor.InvestorId, projectId);

            if (existing is not null)
            {
                existing.AnalysisJson = analysisJson;
                existing.CreatedAt = DateTime.UtcNow;
                _unitOfWork.InvestorAIAnalyses.Update(existing);
            }
            else
            {
                existing = new InvestorAIAnalysis
                {
                    InvestorId = investor.InvestorId,
                    ProjectId = projectId,
                    AnalysisJson = analysisJson,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.InvestorAIAnalyses.AddAsync(existing);
            }

            await _unitOfWork.SaveChangesAsync();
            return MapToResponse(existing, _mapper, project.DevelopmentStage);
        }

        public async Task<InvestorAIAnalysisResponse?> GetAnalysisAsync(int projectId)
        {
            var userId = _userService.GetUserId();
            var investor = await _unitOfWork.Investors.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Investor profile not found.");
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId)
                ?? throw new KeyNotFoundException($"Project {projectId} not found.");

            var analysis = await _unitOfWork.InvestorAIAnalyses
                .GetByInvestorAndProjectAsync(investor.InvestorId, projectId);

            return analysis is null ? null : MapToResponse(analysis, _mapper, project.DevelopmentStage);
        }

        private static InvestorAIAnalysisResponse MapToResponse(
            InvestorAIAnalysis analysis,
            IMapper mapper,
            DevelopmentStage? stage)
        {
            var parsed = DeserializeAnalysisJson(analysis.AnalysisJson);
            var response = mapper.Map<InvestorAIAnalysisResponse>(analysis);
            response.Analysis = parsed;
            response.PotentialScore = parsed?.PotentialScore;
            response.ChaosScore = parsed?.ChaosScore;
            response.ScoreBreakdown = BuildBreakdown(parsed, stage);
            response.InvestmentVerdict = parsed?.InvestmentVerdict ?? string.Empty;
            response.RiskFlags = parsed?.RiskFlags ?? [];
            response.DealBreakers = parsed?.DealBreakers ?? [];
            response.DueDiligenceQuestions = parsed?.DueDiligenceQuestions ?? [];
            response.InvestorNextStep = parsed?.InvestorNextStep ?? string.Empty;
            return response;
        }

        private static GeminiAnalysisResult? DeserializeAnalysisJson(string? analysisJson)
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

        private static int CalculatePotentialScore(GeminiAnalysisResult result, DevelopmentStage? stage)
        {
            static double Normalize(double score) => Math.Clamp(score, 0.0, 2.0);
            var maxPoints = GetStageMaxPointProfile(stage);

            var totalPoints =
                (Normalize(GetComponentScore(result.Team, result.TeamScore)) / 2.0) * maxPoints.Team +
                (Normalize(GetComponentScore(result.Opportunity, result.OpportunityScore)) / 2.0) * maxPoints.Opportunity +
                (Normalize(GetComponentScore(result.Product, result.ProductScore)) / 2.0) * maxPoints.Product +
                (Normalize(GetComponentScore(result.Competition, result.CompetitionScore)) / 2.0) * maxPoints.Competition +
                (Normalize(GetComponentScore(result.Marketing, result.MarketingScore)) / 2.0) * maxPoints.Marketing +
                (Normalize(GetComponentScore(result.Investment, result.InvestmentScore)) / 2.0) * maxPoints.Investment +
                (Normalize(GetComponentScore(result.Other, result.OtherScore)) / 2.0) * maxPoints.Other;

            return (int)Math.Round(totalPoints, MidpointRounding.AwayFromZero);
        }

        private static double GetComponentScore(ComponentEvaluation? component, double fallbackScore)
        {
            return component?.Score > 0 ? component.Score : fallbackScore;
        }

        private static void NormalizeAnalysisResult(GeminiAnalysisResult result)
        {
            static double ClampScore(double value) => Math.Clamp(value, 0.0, 2.0);
            static double ClampConfidence(double value) => Math.Clamp(value, 0.0, 1.0);

            void NormalizeComponent(ComponentEvaluation? component, Action<double> setLegacyScore)
            {
                if (component is null) return;

                component.Score = ClampScore(component.Score);
                component.Confidence = ClampConfidence(component.Confidence);
                component.Evidence ??= [];
                component.MissingData ??= [];
                component.Reason ??= string.Empty;

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
            result.RiskFlags ??= [];
            result.DealBreakers ??= [];
            result.DueDiligenceQuestions ??= [];
            result.InvestmentVerdict ??= string.Empty;
            result.InvestorNextStep ??= string.Empty;
            result.Summary ??= string.Empty;
        }

        private static List<ScoreBreakdownItem> BuildBreakdown(GeminiAnalysisResult? analysis, DevelopmentStage? stage)
        {
            if (analysis is null)
            {
                return [];
            }

            double Normalize(double score) => Math.Clamp(score, 0.0, 2.0);
            double Score(ComponentEvaluation? component, double fallback) => Normalize(GetComponentScore(component, fallback));
            var maxPoints = GetStageMaxPointProfile(stage);

            var team = Score(analysis.Team, analysis.TeamScore);
            var opportunity = Score(analysis.Opportunity, analysis.OpportunityScore);
            var product = Score(analysis.Product, analysis.ProductScore);
            var competition = Score(analysis.Competition, analysis.CompetitionScore);
            var marketing = Score(analysis.Marketing, analysis.MarketingScore);
            var investment = Score(analysis.Investment, analysis.InvestmentScore);
            var other = Score(analysis.Other, analysis.OtherScore);

            return
            [
                new ScoreBreakdownItem { Component = "Team", Weight = maxPoints.Team / 100.0, Score = team, WeightedContribution = Math.Round((team / 2.0) * maxPoints.Team, 2) },
                new ScoreBreakdownItem { Component = "Opportunity", Weight = maxPoints.Opportunity / 100.0, Score = opportunity, WeightedContribution = Math.Round((opportunity / 2.0) * maxPoints.Opportunity, 2) },
                new ScoreBreakdownItem { Component = "Product", Weight = maxPoints.Product / 100.0, Score = product, WeightedContribution = Math.Round((product / 2.0) * maxPoints.Product, 2) },
                new ScoreBreakdownItem { Component = "Competition", Weight = maxPoints.Competition / 100.0, Score = competition, WeightedContribution = Math.Round((competition / 2.0) * maxPoints.Competition, 2) },
                new ScoreBreakdownItem { Component = "Marketing", Weight = maxPoints.Marketing / 100.0, Score = marketing, WeightedContribution = Math.Round((marketing / 2.0) * maxPoints.Marketing, 2) },
                new ScoreBreakdownItem { Component = "Investment", Weight = maxPoints.Investment / 100.0, Score = investment, WeightedContribution = Math.Round((investment / 2.0) * maxPoints.Investment, 2) },
                new ScoreBreakdownItem { Component = "Other", Weight = maxPoints.Other / 100.0, Score = other, WeightedContribution = Math.Round((other / 2.0) * maxPoints.Other, 2) }
            ];
        }

        private static (double Team, double Opportunity, double Product, double Competition, double Marketing, double Investment, double Other)
            GetStageMaxPointProfile(DevelopmentStage? stage)
        {
            return stage switch
            {
                DevelopmentStage.Idea => (20, 25, 30, 5, 10, 5, 5),
                DevelopmentStage.MVP => (25, 20, 25, 10, 10, 5, 5),
                DevelopmentStage.Growth => (20, 15, 15, 10, 20, 15, 5),
                _ => (20, 25, 30, 5, 10, 5, 5)
            };
        }
    }
}
