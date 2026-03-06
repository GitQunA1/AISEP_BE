using AISEP.Common;
using AISEP.DTOs.Requests;
using AISEP.Services.CurrentUser;
using AISEP.Services.Investors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace AISEP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvestorController : ControllerBase
    {
        private readonly IInvestorService _investorService;
        private readonly ICurrentUserService _currentUserService;

        public InvestorController(IInvestorService investorService, ICurrentUserService currentUserService)
        {
            _investorService = investorService;
            _currentUserService = currentUserService;
        }

        // GET api/investor
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] SieveModel model)
        {
            var result = await _investorService.GetAllAsync(model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
        }

        // GET api/investor/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var investor = await _investorService.GetByIdAsync(id);
            if (investor is null)
                return NotFound(ApiResponse<object>.ErrorResponse("Investor not found.", "Not found", 404));

            return Ok(ApiResponse<object>.SuccessResponse(investor, "Success"));
        }

        // GET api/investor/me
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = _currentUserService.GetUserId();
            var investor = await _investorService.GetMyProfileAsync(userId);
            if (investor is null)
                return NotFound(ApiResponse<object>.ErrorResponse("Investor profile not found.", "Not found", 404));

            return Ok(ApiResponse<object>.SuccessResponse(investor, "Success"));
        }

        // POST api/investor
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InvestorRequest dto)
        {
            var userId = _currentUserService.GetUserId();
            var data = await _investorService.CreateAsync(userId, dto);

            if (data is null)
                return Conflict(ApiResponse<object>.ErrorResponse("Investor profile already exists.", "Conflict", 409));

            return CreatedAtAction(nameof(GetById), new { id = data.InvestorId },
                ApiResponse<object>.SuccessResponse(data, "Investor created successfully", 201));
        }

        // PUT api/investor
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] InvestorRequest dto)
        {
            var userId = _currentUserService.GetUserId();
            var data = await _investorService.UpdateAsync(userId, dto);

            if (data is null)
                return NotFound(ApiResponse<object>.ErrorResponse("Investor profile not found.", "Not found", 404));

            return Ok(ApiResponse<object>.SuccessResponse(data, "Investor updated successfully"));
        }
    }
}
