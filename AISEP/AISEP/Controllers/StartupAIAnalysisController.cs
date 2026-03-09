using AISEP.Common;
using AISEP.Services.AI;
using Microsoft.AspNetCore.Mvc;

namespace AISEP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StartupAIAnalysisController : ControllerBase
    {
        private readonly IStartupAIAnalysisService _analysisService;

        public StartupAIAnalysisController(IStartupAIAnalysisService analysisService)
        {
            _analysisService = analysisService;
        }

        /// <summary>Trigger Gemini AI để phân tích và chấm điểm project.</summary>
        [HttpPost("{projectId:int}/analyze")]
        public async Task<IActionResult> Analyze(int projectId)
        {
            var result = await _analysisService.AnalyzeProjectAsync(projectId);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Analysis completed successfully."));
        }

        /// <summary>Lấy kết quả phân tích đã lưu của project.</summary>
        [HttpGet("{projectId:int}")]
        public async Task<IActionResult> GetAnalysis(int projectId)
        {
            var result = await _analysisService.GetAnalysisAsync(projectId);
            if (result is null)
                return NotFound(ApiResponse<object>.ErrorResponse("Analysis not found.", "Not found", 404));

            return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
        }
    }
}
