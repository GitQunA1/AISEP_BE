using AISEP.BLL.Helpers;
using AISEP.BLL.Services.AdminMonitoring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class AdminMonitoringController : ControllerBase
    {
        private readonly IAdminMonitoringService _adminMonitoringService;

        public AdminMonitoringController(IAdminMonitoringService adminMonitoringService)
        {
            _adminMonitoringService = adminMonitoringService;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            var status = await _adminMonitoringService.GetStatusAsync();
            return Ok(ApiResponse<object>.SuccessResponse(status, "Thanh cong"));
        }
    }
}
