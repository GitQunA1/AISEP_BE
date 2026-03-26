using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Connections;
using AISEP.BLL.Services.Investors;
using AISEP.BLL.Services.Startups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/connections")]
    [Authorize]
    public class ConnectionsController : ControllerBase
    {
        private readonly IConnectionService _connectionService;
        private readonly IInvestorService _investorService;
        private readonly IStartupService _startupService;

        public ConnectionsController(
            IConnectionService connectionService,
            IInvestorService investorService,
            IStartupService startupService)
        {
            _connectionService = connectionService;
            _investorService = investorService;
            _startupService = startupService;
        }

        [HttpPost("requests")]
        [Authorize(Roles = "Investor")]
        public async Task<IActionResult> CreateRequest([FromBody] CreateConnectionRequestDto dto)
        {
            var investor = await _investorService.GetMyProfileAsync()
                ?? throw new KeyNotFoundException("Investor profile not found.");

            var data = await _connectionService.CreateRequestAsync(investor.InvestorId, dto);
            return Ok(ApiResponse<object>.SuccessResponse(data, "Connection request created successfully."));
        }

        [HttpPatch("requests/{id:int}/respond")]
        [Authorize(Roles = "Startup")]
        public async Task<IActionResult> RespondRequest(int id, [FromBody] RespondConnectionRequestDto dto)
        {
            var startup = await _startupService.GetMyProfileAsync()
                ?? throw new KeyNotFoundException("Startup profile not found.");

            var data = await _connectionService.RespondToRequestAsync(startup.Id, id, dto.IsAccepted);
            return Ok(ApiResponse<object>.SuccessResponse(data, "Connection request responded successfully."));
        }

        [HttpGet("/api/projects/{id:int}/founder-contact")]
        [Authorize(Roles = "Investor")]
        public async Task<IActionResult> GetFounderContact(int id)
        {
            var investor = await _investorService.GetMyProfileAsync()
                ?? throw new KeyNotFoundException("Investor profile not found.");

            var data = await _connectionService.GetFounderContactAsync(investor.InvestorId, id);
            return Ok(ApiResponse<object>.SuccessResponse(data, "Founder contact retrieved successfully."));
        }
    }
}
