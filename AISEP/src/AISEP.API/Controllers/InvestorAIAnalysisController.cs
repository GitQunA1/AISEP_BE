using AISEP.BLL.Helpers;
using AISEP.BLL.Services.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Investor")]
    public class InvestorAIAnalysisController : ControllerBase
    {
        private readonly IInvestorAIAnalysisService _analysisService;

        public InvestorAIAnalysisController(IInvestorAIAnalysisService analysisService)
        {
            _analysisService = analysisService;
        }

        [HttpPost("{projectId:int}/analyze")]
        public async Task<IActionResult> Analyze(int projectId)
        {
            try
            {
                var result = await _analysisService.AnalyzeProjectForInvestorAsync(projectId);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Investor analysis completed successfully."));
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
        public async Task<IActionResult> GetAnalysis(int projectId)
        {
            try
            {
                var result = await _analysisService.GetAnalysisAsync(projectId);
                if (result is null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Analysis not found.", "Not found", 404));
                }

                return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not found", 404));
            }
        }
    }
}
