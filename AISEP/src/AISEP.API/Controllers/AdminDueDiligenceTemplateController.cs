using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.DueDiligenceTemplates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/admin/due-diligence-template")]
    [Authorize]
    public class AdminDueDiligenceTemplateController : ControllerBase
    {
        private readonly IDueDiligenceTemplateService _dueDiligenceTemplateService;

        public AdminDueDiligenceTemplateController(IDueDiligenceTemplateService dueDiligenceTemplateService)
        {
            _dueDiligenceTemplateService = dueDiligenceTemplateService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff,Startup")]
        public async Task<IActionResult> Get()
        {
            try
            {
                var result = await _dueDiligenceTemplateService.GetAsync();
                return Ok(ApiResponse<object>.SuccessResponse(result, "Due diligence template retrieved successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not found", 404));
            }
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Upsert([FromBody] UpsertDueDiligenceTemplateRequest request)
        {
            var result = await _dueDiligenceTemplateService.UpsertAsync(request);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Due diligence template updated successfully."));
        }
    }
}
