using AISEP.BLL.Common;
using AISEP.BLL.Services.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StartupAIAnalysisController : ControllerBase
    {
        private readonly IStartupAIAnalysisService _analysisService;

        public StartupAIAnalysisController(IStartupAIAnalysisService analysisService)
        {
            _analysisService = analysisService;
        }

        /// <summary>AI để phân tích và chấm điểm project.</summary>
        [HttpPost("{projectId:int}/analyze")]
        public async Task<IActionResult> Analyze(int projectId)
        {
            try
            {
                var result = await _analysisService.AnalyzeProjectAsync(projectId);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Analysis completed successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not found", 404));
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, ApiResponse<object>.ErrorResponse(ex.Message, "AI Service Error", 502));
            }
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
