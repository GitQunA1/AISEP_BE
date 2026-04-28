using System.Text.Json;
using AISEP.BLL.Exceptions;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Users;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AutoMapper;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.BLL.Services.AI
{
    public class InvestorAIAnalysisService : IInvestorAIAnalysisService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IGeminiAiService _geminiAiService;
        private readonly IMapper _mapper;
        private readonly ISieveProcessor _sieveProcessor;

        public InvestorAIAnalysisService(
            IUnitOfWork unitOfWork,
            IUserService userService,
            IGeminiAiService geminiAiService,
            IMapper mapper,
            ISieveProcessor sieveProcessor)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _geminiAiService = geminiAiService;
            _mapper = mapper;
            _sieveProcessor = sieveProcessor;
        }

        public async Task<InvestorAIAnalysisResponse> AnalyzeProjectForInvestorAsync(int projectId)
        {
            var userId = _userService.GetUserId();
            var investor = await _unitOfWork.Investors.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Investor profile not found.");
            EnsureInvestorApproved(investor);

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
            var role = _userService.GetUserRole();
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId)
                ?? throw new KeyNotFoundException($"Project {projectId} not found.");

            InvestorAIAnalysis? analysis;

            if (IsStaffOrAdmin(role))
            {
                analysis = await _unitOfWork.InvestorAIAnalyses.GetLatestByProjectAsync(projectId);
            }
            else if (string.Equals(role, "Investor", StringComparison.OrdinalIgnoreCase))
            {
                var userId = _userService.GetUserId();
                var investor = await _unitOfWork.Investors.GetByUserIdAsync(userId)
                    ?? throw new KeyNotFoundException("Investor profile not found.");

                analysis = await _unitOfWork.InvestorAIAnalyses
                    .GetByInvestorAndProjectAsync(investor.InvestorId, projectId);
            }
            else
            {
                throw new ForbiddenAccessException("You do not have permission to access investor AI analysis.");
            }

            return analysis is null ? null : MapToResponse(analysis, _mapper, project.DevelopmentStage);
        }

        public async Task<PagedResult<InvestorAIAnalysisResponse>> GetAllAnalysesAsync(SieveModel model)
        {
            var role = _userService.GetUserRole();
            IQueryable<InvestorAIAnalysis> query;

            if (IsStaffOrAdmin(role))
            {
                query = _unitOfWork.InvestorAIAnalyses.GetQuery();
            }
            else if (string.Equals(role, "Investor", StringComparison.OrdinalIgnoreCase))
            {
                var userId = _userService.GetUserId();
                var investor = await _unitOfWork.Investors.GetByUserIdAsync(userId)
                    ?? throw new KeyNotFoundException("Investor profile not found.");

                query = _unitOfWork.InvestorAIAnalyses.GetQuery()
                    .Where(x => x.InvestorId == investor.InvestorId);
            }
            else
            {
                throw new ForbiddenAccessException("You do not have permission to access investor AI analysis list.");
            }

            return await PaginationHelper.PaginateAsync(
                query,
                model,
                _sieveProcessor,
                x => MapToResponse(x, _mapper, x.Project?.DevelopmentStage));
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

        private static void EnsureInvestorApproved(Investor investor)
        {
            if (investor.ApprovalStatus != ApprovalStatus.Approved)
                throw new InvalidOperationException("Your investor profile must be approved before using this feature.");
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
            response.ScoreBreakdown = GeminiAnalysisScoringHelper.BuildBreakdown(parsed, stage);

            var normalizedVerdict = NormalizeInvestmentVerdict(parsed?.InvestmentVerdict);
            response.InvestmentVerdict = normalizedVerdict;
            response.RiskFlags = parsed?.RiskFlags ?? [];
            response.DealBreakers = parsed?.DealBreakers ?? [];
            response.DueDiligenceQuestions = parsed?.DueDiligenceQuestions ?? [];
            response.InvestorNextStep = parsed?.InvestorNextStep ?? string.Empty;

            if (response.Analysis is not null)
            {
                response.Analysis.InvestmentVerdict = normalizedVerdict;
            }

            return response;
        }

        private static string NormalizeInvestmentVerdict(string? verdict)
        {
            if (string.IsNullOrWhiteSpace(verdict))
            {
                return string.Empty;
            }

            var normalized = verdict.Trim();
            if (normalized.Equals("Nen dau tu", StringComparison.OrdinalIgnoreCase))
            {
                return "Nen dau tu";
            }
            if (normalized.Equals("Theo doi", StringComparison.OrdinalIgnoreCase))
            {
                return "Theo doi";
            }
            if (normalized.Equals("Tu choi", StringComparison.OrdinalIgnoreCase))
            {
                return "Tu choi";
            }
            if (normalized.Equals("Strong", StringComparison.OrdinalIgnoreCase))
            {
                return "Nen dau tu";
            }
            if (normalized.Equals("Watchlist", StringComparison.OrdinalIgnoreCase))
            {
                return "Theo doi";
            }
            if (normalized.Equals("Pass", StringComparison.OrdinalIgnoreCase))
            {
                return "Tu choi";
            }

            return normalized;
        }

        private static bool IsStaffOrAdmin(string? role)
        {
            return string.Equals(role, "Staff", StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
        }
    }
}
