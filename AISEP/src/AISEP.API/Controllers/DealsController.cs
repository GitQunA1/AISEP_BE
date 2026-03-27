using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Deals;
using AISEP.BLL.Services.Investors;
using AISEP.BLL.Services.Startups;
using AISEP.BLL.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DealsController : ControllerBase
    {
        private readonly IDealService _dealService;
        private readonly IUserService _userService;
        private readonly IInvestorService _investorService;
        private readonly IStartupService _startupService;

        public DealsController(
            IDealService dealService,
            IUserService userService,
            IInvestorService investorService,
            IStartupService startupService)
        {
            _dealService = dealService;
            _userService = userService;
            _investorService = investorService;
            _startupService = startupService;
        }

        [HttpPost]
        [Authorize(Roles = "Investor")]
        public async Task<IActionResult> CreateDeal([FromBody] CreateDealDto dto)
        {
            var userId = _userService.GetUserId();
            var investor = await _investorService.GetMyProfileAsync()
                ?? throw new KeyNotFoundException("Investor profile not found.");

            if (investor.UserId != userId)
            {
                throw new UnauthorizedAccessException("Invalid investor context.");
            }

            var result = await _dealService.CreateDealAsync(investor.InvestorId, dto);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Deal created successfully."));
        }

        [HttpPatch("{id:int}/confirm")]
        [Authorize(Roles = "Startup")]
        public async Task<IActionResult> ConfirmDeal(int id)
        {
            var userId = _userService.GetUserId();
            var startup = await _startupService.GetMyProfileAsync()
                ?? throw new KeyNotFoundException("Startup profile not found.");

            if (startup.UserId != userId)
            {
                throw new UnauthorizedAccessException("Invalid startup context.");
            }

            var result = await _dealService.ConfirmDealAsync(startup.Id, id);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Deal confirmed successfully."));
        }

        [HttpPost("{id:int}/mint-nft")]
        [Authorize(Roles = "Investor")]
        public async Task<IActionResult> MintNft(int id, [FromBody] MintNftRequestDto request)
        {
            var userId = _userService.GetUserId();
            var investor = await _investorService.GetMyProfileAsync()
                ?? throw new KeyNotFoundException("Investor profile not found.");

            if (investor.UserId != userId)
            {
                throw new UnauthorizedAccessException("Invalid investor context.");
            }

            var result = await _dealService.MintNftForDealAsync(id, request);
            return Ok(ApiResponse<object>.SuccessResponse(result, "NFT minted successfully."));
        }

        [HttpGet("my-nfts")]
        [Authorize(Roles = "Investor")]
        public async Task<IActionResult> GetMyNfts([FromQuery] SieveModel sieveModel)
        {
            var userId = _userService.GetUserId();
            var investor = await _investorService.GetMyProfileAsync()
                ?? throw new KeyNotFoundException("Investor profile not found.");

            if (investor.UserId != userId)
            {
                throw new UnauthorizedAccessException("Invalid investor context.");
            }

            var result = await _dealService.GetMyNftsAsync(investor.InvestorId, sieveModel);
            return Ok(ApiResponse<object>.SuccessResponse(result, "NFT deals retrieved successfully."));
        }
    }
}
