using System.Text.Json;
using AISEP.DAL.Common;
using AISEP.BLL.DTOs.Responses;
using AISEP.DAL.Entities;

namespace AISEP.BLL.Services.AI
{
    public class StartupAIAnalysisService : IStartupAIAnalysisService
    {
        private readonly IUnitOfWork      _unitOfWork;
        private readonly IGeminiAiService _geminiAiService;

        public StartupAIAnalysisService(IUnitOfWork unitOfWork, IGeminiAiService geminiAiService)
        {
            _unitOfWork      = unitOfWork;
            _geminiAiService = geminiAiService;
        }

        public async Task<StartupAIAnalysisResponse> AnalyzeProjectAsync(int projectId)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId)
                ?? throw new KeyNotFoundException($"Project {projectId} not found.");

            var documents = await _unitOfWork.Documents.GetByProjectIdAsync(projectId);

            var result      = await _geminiAiService.AnalyzeProjectAsync(project, documents);
            var analysisJson = JsonSerializer.Serialize(result);

            var existing = await _unitOfWork.StartupAIAnalyses.GetByProjectIdAsync(projectId);

            if (existing is not null)
            {
                existing.PotentialScore    = result.PotentialScore;
                existing.ChaosScore        = result.ChaosScore;
                existing.AnalysisJson      = analysisJson;
                existing.IsEligibleStartup = result.IsEligibleStartup;
                existing.EligibilityReason = result.EligibilityReason;
                existing.CreatedAt         = DateTime.UtcNow;
                _unitOfWork.StartupAIAnalyses.Update(existing);
            }
            else
            {
                existing = new StartupAIAnalysis
                {
                    ProjectId         = projectId,
                    PotentialScore    = result.PotentialScore,
                    ChaosScore        = result.ChaosScore,
                    AnalysisJson      = analysisJson,
                    IsEligibleStartup = result.IsEligibleStartup,
                    EligibilityReason = result.EligibilityReason,
                    CreatedAt         = DateTime.UtcNow
                };
                await _unitOfWork.StartupAIAnalyses.AddAsync(existing);
            }

            await _unitOfWork.SaveChangesAsync();

            return MapToResponse(existing);
        }

        public async Task<StartupAIAnalysisResponse?> GetAnalysisAsync(int projectId)
        {
            var analysis = await _unitOfWork.StartupAIAnalyses.GetByProjectIdAsync(projectId);
            return analysis is null ? null : MapToResponse(analysis);
        }

        private static StartupAIAnalysisResponse MapToResponse(StartupAIAnalysis a) => new()
        {
            EvaluationId      = a.EvaluationId,
            ProjectId         = a.ProjectId,
            PotentialScore    = a.PotentialScore,
            ChaosScore        = a.ChaosScore,
            AnalysisJson      = a.AnalysisJson,
            IsEligibleStartup = a.IsEligibleStartup,
            EligibilityReason = a.EligibilityReason,
            CreatedAt         = a.CreatedAt
        };
    }
}
