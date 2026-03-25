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

            await ConsumeAiQuotaAsync(userId);

            var project = await _unitOfWork.Projects.GetByIdAsync(projectId)
                ?? throw new KeyNotFoundException($"Project {projectId} not found.");
            if (project.Status != ProjectStatus.Approved)
            {
                throw new InvalidOperationException("Investor AI analysis is only available when project status is Approved.");
            }

            var documents = (await _unitOfWork.Documents.GetByProjectIdAsync(projectId)).ToList();
            var result = await _geminiAiService.AnalyzeProjectForInvestorAsync(project, documents);
            GeminiAnalysisScoringHelper.NormalizeAnalysisResult(result, includeInvestorFields: true);
            result.PotentialScore = GeminiAnalysisScoringHelper.CalculatePotentialScore(result, project.DevelopmentStage);
            result.PotentialScore = GeminiAnalysisScoringHelper.ApplyDataQualitySanityCap(result.PotentialScore, result, project);

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

        private async Task ConsumeAiQuotaAsync(int userId)
        {
            var subscription = await _unitOfWork.Subscriptions.GetLatestActiveAsync(userId)
                ?? throw new InvalidOperationException("No active subscription.");

            var package = await _unitOfWork.Packages.GetByIdAsync(subscription.PackageId)
                ?? throw new KeyNotFoundException("Package not found.");

            AiQuotaPolicy.EnsureAiQuotaNotExceeded(subscription, package);

            subscription.UsedAiRequests += 1;
            _unitOfWork.Subscriptions.Update(subscription);
            await _unitOfWork.SaveChangesAsync();
        }

        private static InvestorAIAnalysisResponse MapToResponse(
            InvestorAIAnalysis analysis,
            IMapper mapper,
            DevelopmentStage? stage)
        {
            var parsed = GeminiAnalysisScoringHelper.DeserializeAnalysisJson(analysis.AnalysisJson);
            var response = mapper.Map<InvestorAIAnalysisResponse>(analysis);
            response.Analysis = parsed;
            response.PotentialScore = parsed?.PotentialScore;
            response.ChaosScore = parsed?.ChaosScore;
            response.ScoreBreakdown = GeminiAnalysisScoringHelper.BuildBreakdown(parsed, stage);
            response.InvestmentVerdict = parsed?.InvestmentVerdict ?? string.Empty;
            response.RiskFlags = parsed?.RiskFlags ?? [];
            response.DealBreakers = parsed?.DealBreakers ?? [];
            response.DueDiligenceQuestions = parsed?.DueDiligenceQuestions ?? [];
            response.InvestorNextStep = parsed?.InvestorNextStep ?? string.Empty;
            return response;
        }
    }
}
