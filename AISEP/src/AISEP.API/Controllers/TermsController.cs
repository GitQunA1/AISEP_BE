using AISEP.BLL.Helpers;
using AISEP.BLL.Services.SystemTerms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/terms")]
    public class TermsController : ControllerBase
    {
        private readonly ISystemTermService _systemTermService;

        public TermsController(ISystemTermService systemTermService)
        {
            _systemTermService = systemTermService;
        }

        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActive()
        {
            var result = await _systemTermService.GetActiveAsync();
            return Ok(ApiResponse<object>.SuccessResponse(result, "Active system terms retrieved successfully."));
        }
    }
}
