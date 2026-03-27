using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.UserReports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserReportsController : ControllerBase
    {
        private readonly IUserReportService _userReportService;

        public UserReportsController(IUserReportService userReportService)
        {
            _userReportService = userReportService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserReportRequest request)
        {
            var result = await _userReportService.CreateAsync(request);
            return Ok(ApiResponse<object>.SuccessResponse(result, "User report created successfully."));
        }

        [HttpPatch("{id:int}/resolve-valid")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> ResolveValid(int id)
        {
            var result = await _userReportService.ResolveAsValidAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(result, "User report marked as valid."));
        }

        [HttpPatch("{id:int}/resolve-false")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> ResolveFalse(int id)
        {
            var result = await _userReportService.ResolveAsFalseAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(result, "User report marked as false report."));
        }
    }
}
