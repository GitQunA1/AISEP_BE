using AISEP.BLL.Helpers;
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

       
        [HttpPost("{projectId:int}/analyze")]
        [Authorize(Roles = "Startup")]
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
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<object>.ErrorResponse(ex.Message, "Conflict", 409));
            }
        }

      
        [HttpGet("{projectId:int}")]
        [Authorize(Roles = "Startup,Staff,Admin")]
        public async Task<IActionResult> GetAnalysis(int projectId)
        {
            try
            {
                var result = await _analysisService.GetAnalysisAsync(projectId);
                if (result is null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Analysis not found.", "Not found", 404));

                return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not found", 404));
            }
        }

        /// Sàng lọc nhanh tài liệu đính kèm có thuộc về dự án hay không.
        [HttpPost("{projectId:int}/eligibility-evaluate-staff")]
        [Authorize(Roles = "Staff, Admin")]
        public async Task<IActionResult> EvaluateEligibility(int projectId)
        {
            try
            {
                var result = await _analysisService.EvaluateEligibilityAsync(projectId);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Eligibility evaluation completed successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not found", 404));
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, ApiResponse<object>.ErrorResponse(ex.Message, "AI Service Error", 502));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<object>.ErrorResponse(ex.Message, "Conflict", 409));
            }
        }

        [HttpGet("{projectId:int}/eligibility-evaluate-staff")]
        [Authorize(Roles = "Staff, Admin")]
        public async Task<IActionResult> GetEligibilityEvaluation(int projectId)
        {
            try
            {
                var result = await _analysisService.GetEligibilityEvaluationAsync(projectId);
                if (result is null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Eligibility evaluation not found.", "Not found", 404));

                return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not found", 404));
            }
        }
    }
}
