using System.Text.Json;
using System.Text;
using AISEP.BLL.Exceptions;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.PdfExtraction;
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
        private readonly IOpenAiService _openAiService;
        private readonly IPdfExtractionService _pdfExtractionService;
        private readonly IMapper _mapper;
        private readonly ISieveProcessor _sieveProcessor;

        public InvestorAIAnalysisService(
            IUnitOfWork unitOfWork,
            IUserService userService,
            IOpenAiService openAiService,
            IPdfExtractionService pdfExtractionService,
            IMapper mapper,
            ISieveProcessor sieveProcessor)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _openAiService = openAiService;
            _pdfExtractionService = pdfExtractionService;
            _mapper = mapper;
            _sieveProcessor = sieveProcessor;
        }

        public async Task<InvestorAIAnalysisResponse> AnalyzeProjectForInvestorAsync(int projectId)
        {
            var userId = _userService.GetUserId();
            var investor = await _unitOfWork.Investors.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Investor profile not found.");
            EnsureInvestorApproved(investor);

            var project = await _unitOfWork.Projects.GetByIdAsync(projectId)
                ?? throw new KeyNotFoundException($"Project {projectId} not found.");
            if (project.Status != ProjectStatus.Approved)
            {
                throw new InvalidOperationException("Investor AI analysis is only available when project status is Approved.");
            }

            var baseScore = await CalculateBaseScoreAsync(project);
            await ConsumeAiQuotaAsync(userId);

            var documentText = await ExtractProjectPdfTextAsync(projectId);
            var result = await _openAiService.AnalyzeProjectForInvestorAsync(project, baseScore, documentText);
            var report = AIAnalysisReportBuilder.Build(result, baseScore);

            var analysisJson = JsonSerializer.Serialize(report);
            var existing = await _unitOfWork.InvestorAIAnalyses
                .GetByInvestorAndProjectAsync(investor.InvestorId, projectId);

            if (existing is not null)
            {
                existing.BaseScore = report.TotalBaseScore;
                existing.AIAdjustmentScore = report.TotalAIAdjustmentScore;
                existing.FinalPotentialScore = report.TotalFinalScore;
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
                    BaseScore = report.TotalBaseScore,
                    AIAdjustmentScore = report.TotalAIAdjustmentScore,
                    FinalPotentialScore = report.TotalFinalScore,
                    AnalysisJson = analysisJson,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.InvestorAIAnalyses.AddAsync(existing);
            }

            await _unitOfWork.SaveChangesAsync();
            return MapToResponse(existing, _mapper);
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

            return analysis is null ? null : MapToResponse(analysis, _mapper);
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
                x => MapToResponse(x, _mapper));
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

        private static void EnsureInvestorApproved(Investor investor)
        {
            if (investor.ApprovalStatus != ApprovalStatus.Approved)
                throw new InvalidOperationException("Your investor profile must be approved before using this feature.");
        }

        private static InvestorAIAnalysisResponse MapToResponse(
            InvestorAIAnalysis analysis,
            IMapper mapper)
        {
            var response = mapper.Map<InvestorAIAnalysisResponse>(analysis);
            response.Analysis = DeserializeAnalysisJson(analysis.AnalysisJson);
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

        private static bool IsStaffOrAdmin(string? role)
        {
            return string.Equals(role, "Staff", StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
        }
    }
}
