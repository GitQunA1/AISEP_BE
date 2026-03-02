using AISEP.DTOs;
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
            return Ok(new { success = true, data = result });
        }

        // GET api/investor/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var investor = await _investorService.GetByIdAsync(id);
            if (investor is null)
                return NotFound(new { success = false, message = "Investor not found." });

            return Ok(new { success = true, data = investor });
        }

        // GET api/investor/me
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = _currentUserService.GetUserId();
            var investor = await _investorService.GetMyProfileAsync(userId);
            if (investor is null)
                return NotFound(new { success = false, message = "Investor profile not found." });

            return Ok(new { success = true, data = investor });
        }

        // POST api/investor
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InvestorDto dto)
        {
            var userId = _currentUserService.GetUserId();
            var data = await _investorService.CreateAsync(userId, dto);

            if (data is null)
                return Conflict(new { success = false, message = "Investor profile already exists." });

            return CreatedAtAction(nameof(GetById), new { id = data.InvestorId },
                new { success = true, data });
        }

        // PUT api/investor
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] InvestorDto dto)
        {
            var userId = _currentUserService.GetUserId();
            var data = await _investorService.UpdateAsync(userId, dto);

            if (data is null)
                return NotFound(new { success = false, message = "Investor profile not found." });

            return Ok(new { success = true, data });
        }
    }
}
