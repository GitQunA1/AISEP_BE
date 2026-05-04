using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AISEP.BLL.Exceptions;
using AISEP.BLL.Helpers;
using AISEP.DAL.Common;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Services.PdfExtraction;
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
        private readonly IOpenAiService _openAiService;
        private readonly IPdfExtractionService _pdfExtractionService;
        private readonly IMapper           _mapper;

        public StartupAIAnalysisService(
            IUnitOfWork unitOfWork,
            IUserService userService,
            IOpenAiService openAiService,
            IPdfExtractionService pdfExtractionService,
            IMapper mapper)
        {
            _unitOfWork      = unitOfWork;
            _userService     = userService;
            _openAiService   = openAiService;
            _pdfExtractionService = pdfExtractionService;
            _mapper          = mapper;
        }

        public async Task<StartupAIAnalysisResponse> AnalyzeProjectAsync(int projectId)
        {
            var project = await EnsureProjectBelongsToCurrentStartupAsync(projectId, requireApprovedStartup: true);
            if (project.Status != ProjectStatus.Draft)
            {
                throw new InvalidOperationException("AI analysis is only available when project status is Draft.");
            }

            var baseScore = await CalculateBaseScoreAsync(project);
            await ConsumeAiQuotaAsync(project.StartupId);

            var documentText = await ExtractProjectPdfTextAsync(projectId);
            var result      = await _openAiService.AnalyzeProjectAsync(project, baseScore, documentText);
            var report = AIAnalysisReportBuilder.Build(result, baseScore);
            var analysisJson = JsonSerializer.Serialize(report);

            var existing = await _unitOfWork.StartupAIAnalyses.GetByProjectIdAsync(projectId);

            if (existing is not null)
            {
                existing.BaseScore = report.TotalBaseScore;
                existing.AIAdjustmentScore = report.TotalAIAdjustmentScore;
                existing.FinalPotentialScore = report.TotalFinalScore;
                existing.AnalysisJson = analysisJson;
                existing.IsEligibleStartup = null;
                existing.EligibilityReason = null;
                existing.CreatedAt = DateTime.UtcNow;
                _unitOfWork.StartupAIAnalyses.Update(existing);
            }
            else
            {
                existing = new StartupAIAnalysis
                {
                    ProjectId = projectId,
                    BaseScore = report.TotalBaseScore,
                    AIAdjustmentScore = report.TotalAIAdjustmentScore,
                    FinalPotentialScore = report.TotalFinalScore,
                    AnalysisJson = analysisJson,
                    IsEligibleStartup = null,
                    EligibilityReason = null,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.StartupAIAnalyses.AddAsync(existing);
            }

            await _unitOfWork.SaveChangesAsync();

            return MapToResponse(existing, _mapper);
        }

        public async Task<StartupAIAnalysisResponse?> GetAnalysisAsync(int projectId)
        {
            var role = _userService.GetUserRole();
            var project = IsStaffOrAdmin(role)
                ? await _unitOfWork.Projects.GetByIdAsync(projectId)
                    ?? throw new KeyNotFoundException($"Project {projectId} not found.")
                : await EnsureProjectBelongsToCurrentStartupAsync(projectId);

            var analysis = await _unitOfWork.StartupAIAnalyses.GetByProjectIdAsync(projectId);
            return analysis is null ? null : MapToResponse(analysis, _mapper);
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
            var result = await _openAiService.EvaluateStartupEligibilityAsync(project, documents);

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

        private async Task<Project> EnsureProjectBelongsToCurrentStartupAsync(int projectId, bool requireApprovedStartup = false)
        {
            var userId = _userService.GetUserId();
            var startup = await _unitOfWork.Startups.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Startup profile not found for this account.");
            if (requireApprovedStartup)
            {
                EnsureStartupApproved(startup);
            }

            var project = await _unitOfWork.Projects.GetByIdAsync(projectId)
                ?? throw new KeyNotFoundException($"Project {projectId} not found.");

            if (project.StartupId != startup.StartupId)
            {
                throw new ForbiddenAccessException("You do not have permission to analyze this project.");
            }

            return project;
        }

        private static void EnsureStartupApproved(Startup startup)
        {
            if (startup.ApprovalStatus != ApprovalStatus.Approved)
                throw new InvalidOperationException("Your startup profile must be approved before using this feature.");
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

        private async Task<ScorecardBaseScoreResult> CalculateBaseScoreAsync(Project project)
        {
            if (project.Scorecard is null)
            {
                throw new InvalidOperationException("Project scorecard is required before running AI analysis.");
            }

            var weightConfig = await _unitOfWork.ScorecardWeightConfigs.GetDefaultAsync()
                ?? throw new InvalidOperationException("Default scorecard weight config is not configured.");

            return ProjectScoringHelper.CalculateBaseScoreBreakdown(project.Scorecard, weightConfig);
        }

        private async Task<string> ExtractProjectPdfTextAsync(int projectId)
        {
            const int maxPromptDocumentCharacters = 15_000;
            const string truncatedSuffix = "... [Đã cắt bớt do quá dài]";

            var documents = await _unitOfWork.Documents.GetByProjectIdAsync(projectId);
            var pdfDocuments = documents
                .Where(IsPdfDocument)
                .ToList();

            if (pdfDocuments.Count == 0)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            foreach (var document in pdfDocuments)
            {
                if (builder.Length >= maxPromptDocumentCharacters)
                {
                    break;
                }

                var extractedText = await _pdfExtractionService.ExtractTextFromPdfUrlAsync(document.FileUrl);
                if (string.IsNullOrWhiteSpace(extractedText))
                {
                    continue;
                }

                builder.AppendLine($"Tài liệu: {document.DocumentType} - {document.FileName}");
                builder.AppendLine(extractedText);
                builder.AppendLine();
            }

            var text = builder.ToString().Trim();
            if (text.Length <= maxPromptDocumentCharacters)
            {
                return text;
            }

            var allowedLength = Math.Max(0, maxPromptDocumentCharacters - truncatedSuffix.Length);
            return text[..allowedLength].TrimEnd() + truncatedSuffix;
        }

        private static bool IsPdfDocument(Document document)
        {
            return HasPdfExtension(document.FileName) || HasPdfExtension(document.FileUrl);
        }

        private static bool HasPdfExtension(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var withoutQuery = value.Split('?', 2)[0];
            return withoutQuery.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
        }

        private static StartupAIAnalysisResponse MapToResponse(StartupAIAnalysis a, IMapper mapper)
        {
            var response = mapper.Map<StartupAIAnalysisResponse>(a);
            response.Analysis = DeserializeAnalysisJson(a.AnalysisJson);
            return response;
        }

        private static AIAnalysisReportDto? DeserializeAnalysisJson(string? analysisJson)
        {
            if (string.IsNullOrWhiteSpace(analysisJson))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<AIAnalysisReportDto>(
                    analysisJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static string NormalizeEligibilityReason(string? reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return "Tài liệu đính kèm không đủ thông tin để đối chiếu với nội dung dự án.";
            }

            var normalized = Regex.Replace(reason.Trim(), @"\s+", " ");
            var sentences = Regex.Matches(normalized, @"[^.!?]+[.!?]?")
                .Select(m => m.Value.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Take(3)
                .ToList();

            if (sentences.Count == 0)
            {
                return "Tài liệu đính kèm không đủ thông tin để đối chiếu với nội dung dự án.";
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
