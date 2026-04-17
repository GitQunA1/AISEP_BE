using System.Text.Json;
using System.Text.RegularExpressions;
using AISEP.BLL.Exceptions;
using AISEP.DAL.Common;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Services.Users;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AutoMapper;

namespace AISEP.BLL.Services.AI
{
    public class StartupAIAnalysisService : IStartupAIAnalysisService
    {
        private readonly IUnitOfWork      _unitOfWork;
        private readonly IUserService     _userService;
        private readonly IGeminiAiService _geminiAiService;
        private readonly IMapper           _mapper;

        public StartupAIAnalysisService(
            IUnitOfWork unitOfWork,
            IUserService userService,
            IGeminiAiService geminiAiService,
            IMapper mapper)
        {
            _unitOfWork      = unitOfWork;
            _userService     = userService;
            _geminiAiService = geminiAiService;
            _mapper          = mapper;
        }

        public async Task<StartupAIAnalysisResponse> AnalyzeProjectAsync(int projectId)
        {
            var project = await EnsureProjectBelongsToCurrentStartupAsync(projectId);
            if (project.Status != ProjectStatus.Draft)
            {
                throw new InvalidOperationException("AI analysis is only available when project status is Draft.");
            }

            await ConsumeAiQuotaAsync(project.StartupId);

            var documents = (await _unitOfWork.Documents.GetByProjectIdAsync(projectId)).ToList();

            var result      = await _geminiAiService.AnalyzeProjectAsync(project, documents);
            GeminiAnalysisScoringHelper.NormalizeAnalysisResult(result, includeInvestorFields: false);
            result.PotentialScore = GeminiAnalysisScoringHelper.CalculatePotentialScore(result, project.DevelopmentStage);
            result.PotentialScore = GeminiAnalysisScoringHelper.ApplyDataQualitySanityCap(result.PotentialScore, result, project);
            var analysisJson = JsonSerializer.Serialize(result);

            var existing = await _unitOfWork.StartupAIAnalyses.GetByProjectIdAsync(projectId);

            if (existing is not null)
            {
                existing.PotentialScore    = result.PotentialScore;
                existing.AnalysisJson      = analysisJson;
                existing.IsEligibleStartup = null;
                existing.EligibilityReason = null;
                existing.CreatedAt         = DateTime.UtcNow;
                _unitOfWork.StartupAIAnalyses.Update(existing);
            }
            else
            {
                existing = new StartupAIAnalysis
                {
                    ProjectId         = projectId,
                    PotentialScore    = result.PotentialScore,
                    AnalysisJson      = analysisJson,
                    IsEligibleStartup = null,
                    EligibilityReason = null,
                    CreatedAt         = DateTime.UtcNow
                };
                await _unitOfWork.StartupAIAnalyses.AddAsync(existing);
            }

            await _unitOfWork.SaveChangesAsync();

            return MapToResponse(existing, _mapper, project.DevelopmentStage);
        }

        public async Task<StartupAIAnalysisResponse?> GetAnalysisAsync(int projectId)
        {
            var role = _userService.GetUserRole();
            var project = IsStaffOrAdmin(role)
                ? await _unitOfWork.Projects.GetByIdAsync(projectId)
                    ?? throw new KeyNotFoundException($"Project {projectId} not found.")
                : await EnsureProjectBelongsToCurrentStartupAsync(projectId);

            var analysis = await _unitOfWork.StartupAIAnalyses.GetByProjectIdAsync(projectId);
            return analysis is null ? null : MapToResponse(analysis, _mapper, project.DevelopmentStage);
        }

        public async Task<StartupEligibilityResponse> EvaluateEligibilityAsync(int projectId)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId)
                ?? throw new KeyNotFoundException($"Project {projectId} not found.");

            if (project.Status != ProjectStatus.Pending)
            {
                throw new InvalidOperationException("Chỉ được đánh giá eligibility khi dự án đang ở trạng thái Pending.");
            }

            var documents = (await _unitOfWork.Documents.GetByProjectIdAsync(projectId)).ToList();
            var result = await _geminiAiService.EvaluateStartupEligibilityAsync(project, documents);

            var normalizedReason = NormalizeEligibilityReason(result.EligibilityReason);
            var eligibilityJson = JsonSerializer.Serialize(new StartupEligibilityResponse
            {
                IsEligibleStartup = result.IsEligibleStartup,
                EligibilityReason = normalizedReason
            });

            var existing = await _unitOfWork.StartupAIAnalyses.GetByProjectIdAsync(projectId);

            if (existing is not null)
            {
                existing.IsEligibleStartup = result.IsEligibleStartup;
                existing.EligibilityReason = normalizedReason;
                existing.AnalysisJson = eligibilityJson;
                existing.CreatedAt = DateTime.UtcNow;
                _unitOfWork.StartupAIAnalyses.Update(existing);
            }
            else
            {
                existing = new StartupAIAnalysis
                {
                    ProjectId = projectId,
                    IsEligibleStartup = result.IsEligibleStartup,
                    EligibilityReason = normalizedReason,
                    AnalysisJson = eligibilityJson,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.StartupAIAnalyses.AddAsync(existing);
            }

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<StartupEligibilityResponse>(existing);
        }

        public async Task<StartupEligibilityResponse?> GetEligibilityEvaluationAsync(int projectId)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId)
                ?? throw new KeyNotFoundException($"Project {projectId} not found.");

            var analysis = await _unitOfWork.StartupAIAnalyses.GetByProjectIdAsync(projectId);
            if (analysis is null || !analysis.IsEligibleStartup.HasValue || string.IsNullOrWhiteSpace(analysis.EligibilityReason))
            {
                return null;
            }

            return new StartupEligibilityResponse
            {
                IsEligibleStartup = analysis.IsEligibleStartup.Value,
                EligibilityReason = analysis.EligibilityReason
            };
        }

        private async Task<Project> EnsureProjectBelongsToCurrentStartupAsync(int projectId)
        {
            var userId = _userService.GetUserId();
            var startup = await _unitOfWork.Startups.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Startup profile not found for this account.");

            var project = await _unitOfWork.Projects.GetByIdAsync(projectId)
                ?? throw new KeyNotFoundException($"Project {projectId} not found.");

            if (project.StartupId != startup.StartupId)
            {
                throw new ForbiddenAccessException("You do not have permission to analyze this project.");
            }

            return project;
        }

        private async Task ConsumeAiQuotaAsync(int startupId)
        {
            var startup = await _unitOfWork.Startups.GetByIdAsync(startupId)
                ?? throw new KeyNotFoundException("Startup profile not found.");

            var subscription = await _unitOfWork.Subscriptions.GetLatestActiveAsync(startup.UserId)
                ?? throw new InvalidOperationException("No active subscription.");

            var package = await _unitOfWork.Packages.GetByIdAsync(subscription.PackageId)
                ?? throw new KeyNotFoundException("Package not found.");

            AiQuotaPolicy.EnsureAiQuotaNotExceeded(subscription, package);

            subscription.UsedAiRequests += 1;
            _unitOfWork.Subscriptions.Update(subscription);
            await _unitOfWork.SaveChangesAsync();
        }

        private static StartupAIAnalysisResponse MapToResponse(StartupAIAnalysis a, IMapper mapper, DevelopmentStage? stage)
        {
            var parsedAnalysis = GeminiAnalysisScoringHelper.DeserializeAnalysisJson(a.AnalysisJson);
            var response = mapper.Map<StartupAIAnalysisResponse>(a);
            response.Analysis = parsedAnalysis;
            response.ScoreBreakdown = GeminiAnalysisScoringHelper.BuildBreakdown(parsedAnalysis, stage);
            return response;
        }

        private static string NormalizeEligibilityReason(string? reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return "Dự án chưa có đủ dữ liệu rõ ràng để kết luận theo bộ tiêu chí IDEO và Lean Startup.";
            }

            var normalized = Regex.Replace(reason.Trim(), @"\s+", " ");
            var sentences = Regex.Matches(normalized, @"[^.!?]+[.!?]?")
                .Select(m => m.Value.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Take(3)
                .ToList();

            if (sentences.Count == 0)
            {
                return "Dự án chưa có đủ dữ liệu rõ ràng để kết luận theo bộ tiêu chí IDEO và Lean Startup.";
            }

            return string.Join(" ", sentences);
        }

        private static bool IsStaffOrAdmin(string? role)
        {
            return string.Equals(role, "Staff", StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
        }
    }
}
