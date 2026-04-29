using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Deals;
using AISEP.BLL.Services.Investors;
using AISEP.BLL.Services.Startups;
using AISEP.BLL.Services.Users;
using AISEP.BLL.Exceptions;
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

        [HttpGet]
        [Authorize(Roles = "Investor,Startup,Staff,Admin")]
        public async Task<IActionResult> GetDeals([FromQuery] SieveModel model)
        {
            var userId = _userService.GetUserId();

            if (User.IsInRole("Investor"))
            {
                var investor = await _investorService.GetMyProfileAsync()
                    ?? throw new KeyNotFoundException("Investor profile not found.");

                if (investor.UserId != userId)
                {
                    throw new UnauthorizedAccessException("Invalid investor context.");
                }

                var investorDeals = await _dealService.GetInvestorDealsAsync(investor.InvestorId, model);
                return Ok(ApiResponse<object>.SuccessResponse(investorDeals, "Deals retrieved successfully."));
            }

            if (User.IsInRole("Startup"))
            {
                var startup = await _startupService.GetMyProfileAsync()
                    ?? throw new KeyNotFoundException("Startup profile not found.");

                if (startup.UserId != userId)
                {
                    throw new UnauthorizedAccessException("Invalid startup context.");
                }

                var startupDeals = await _dealService.GetStartupDealsAsync(startup.Id, model);
                return Ok(ApiResponse<object>.SuccessResponse(startupDeals, "Deals retrieved successfully."));
            }

            if (User.IsInRole("Staff") || User.IsInRole("Admin"))
            {
                var allDeals = await _dealService.GetDealsAsync(model);
                return Ok(ApiResponse<object>.SuccessResponse(allDeals, "Deals retrieved successfully."));
            }

            throw new UnauthorizedAccessException("Role is not allowed to access deals.");
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Investor,Startup,Staff,Admin")]
        public async Task<IActionResult> GetDealById(int id)
        {
            var userId = _userService.GetUserId();

            if (User.IsInRole("Investor"))
            {
                var investor = await _investorService.GetMyProfileAsync()
                    ?? throw new KeyNotFoundException("Investor profile not found.");

                if (investor.UserId != userId)
                {
                    throw new UnauthorizedAccessException("Invalid investor context.");
                }

                var deal = await _dealService.GetDealByIdAsync(id);
                if (deal.InvestorId != investor.InvestorId)
                    throw new ForbiddenAccessException("You do not have permission to access this deal.");

                return Ok(ApiResponse<object>.SuccessResponse(deal, "Deal retrieved successfully."));
            }

            if (User.IsInRole("Startup"))
            {
                var startup = await _startupService.GetMyProfileAsync()
                    ?? throw new KeyNotFoundException("Startup profile not found.");

                if (startup.UserId != userId)
                {
                    throw new UnauthorizedAccessException("Invalid startup context.");
                }

                var deal = await _dealService.GetDealByIdAsync(id);
                if (deal.StartupId != startup.Id)
                    throw new ForbiddenAccessException("You do not have permission to access this deal.");

                return Ok(ApiResponse<object>.SuccessResponse(deal, "Deal retrieved successfully."));
            }

            if (User.IsInRole("Staff") || User.IsInRole("Admin"))
            {
                var deal = await _dealService.GetDealByIdAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse(deal, "Deal retrieved successfully."));
            }

            throw new UnauthorizedAccessException("Role is not allowed to access deals.");
        }

        [HttpPost]
        [Authorize(Roles = "Investor,Startup")]
        public async Task<IActionResult> CreateDeal([FromForm] CreateDealDto dto)
        {
            var userId = _userService.GetUserId();

            if (User.IsInRole("Investor"))
            {
                var investor = await _investorService.GetMyProfileAsync()
                    ?? throw new KeyNotFoundException("Investor profile not found.");

                if (investor.UserId != userId)
                {
                    throw new UnauthorizedAccessException("Invalid investor context.");
                }

                var result = await _dealService.CreateDealForInvestorAsync(investor.InvestorId, dto);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Deal created successfully."));
            }

            if (User.IsInRole("Startup"))
            {
                var startup = await _startupService.GetMyProfileAsync()
                    ?? throw new KeyNotFoundException("Startup profile not found.");

                if (startup.UserId != userId)
                {
                    throw new UnauthorizedAccessException("Invalid startup context.");
                }

                var result = await _dealService.CreateDealForStartupAsync(startup.Id, dto);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Deal created successfully."));
            }

            throw new UnauthorizedAccessException("Role is not allowed to create deals.");
        }

        [HttpPatch("{id:int}/verify")]
        [Authorize(Roles = "Investor,Startup")]
        public async Task<IActionResult> VerifyDeal(int id, [FromBody] VerifyDealRequestDto dto)
        {
            var userId = _userService.GetUserId();

            if (User.IsInRole("Investor"))
            {
                var investor = await _investorService.GetMyProfileAsync()
                    ?? throw new KeyNotFoundException("Investor profile not found.");

                if (investor.UserId != userId)
                {
                    throw new UnauthorizedAccessException("Invalid investor context.");
                }

                var result = await _dealService.VerifyDealForInvestorAsync(investor.InvestorId, id, dto);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Deal verification submitted successfully."));
            }

            if (User.IsInRole("Startup"))
            {
                var startup = await _startupService.GetMyProfileAsync()
                    ?? throw new KeyNotFoundException("Startup profile not found.");

                if (startup.UserId != userId)
                {
                    throw new UnauthorizedAccessException("Invalid startup context.");
                }

                var result = await _dealService.VerifyDealForStartupAsync(startup.Id, id, dto);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Deal verification submitted successfully."));
            }

            throw new UnauthorizedAccessException("Role is not allowed to verify deals.");
        }

        [HttpPut("{id:int}/staff-review")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> StaffReviewDeal(int id, [FromBody] StaffReviewDealRequestDto dto)
        {
            var result = await _dealService.StaffReviewDealAsync(id, dto);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Deal review submitted successfully."));
        }

        [HttpPut("{id:int}/reupload")]
        [Authorize(Roles = "Investor,Startup")]
        public async Task<IActionResult> ReuploadDealEvidence(int id, [FromForm] ReuploadDealEvidenceDto dto)
        {
            var userId = _userService.GetUserId();

            if (User.IsInRole("Investor"))
            {
                var investor = await _investorService.GetMyProfileAsync()
                    ?? throw new KeyNotFoundException("Investor profile not found.");

                if (investor.UserId != userId)
                {
                    throw new UnauthorizedAccessException("Invalid investor context.");
                }

                var result = await _dealService.ReuploadDealEvidenceForInvestorAsync(investor.InvestorId, id, dto);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Deal evidence updated successfully."));
            }

            if (User.IsInRole("Startup"))
            {
                var startup = await _startupService.GetMyProfileAsync()
                    ?? throw new KeyNotFoundException("Startup profile not found.");

                if (startup.UserId != userId)
                {
                    throw new UnauthorizedAccessException("Invalid startup context.");
                }

                var result = await _dealService.ReuploadDealEvidenceForStartupAsync(startup.Id, id, dto);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Deal evidence updated successfully."));
            }

            throw new UnauthorizedAccessException("Role is not allowed to reupload deal evidence.");
        }

        [HttpGet("{id:int}/verify-onchain")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> VerifyOnChain(int id)
        {
            var result = await _dealService.GetDealOnChainVerificationAsync(id);
            var message = string.IsNullOrWhiteSpace(result.Message)
                ? "Deal verification on-chain completed."
                : result.Message;

            return Ok(ApiResponse<object>.SuccessResponse(result, message));
        }
    }
}
