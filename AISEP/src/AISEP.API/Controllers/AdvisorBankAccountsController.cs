using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Exceptions;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.AdvisorBankAccounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/advisor-bank-accounts")]
    public class AdvisorBankAccountsController : ControllerBase
    {
        private readonly IAdvisorBankAccountService _advisorBankAccountService;

        public AdvisorBankAccountsController(IAdvisorBankAccountService advisorBankAccountService)
        {
            _advisorBankAccountService = advisorBankAccountService;
        }

        [HttpGet]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> GetAll([FromQuery] SieveModel model)
        {
            var result = await _advisorBankAccountService.GetAllAsync(model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Advisor,Staff,Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _advisorBankAccountService.GetByIdAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not Found", 404));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message, "Forbidden", 403));
            }
        }

        [HttpGet("me")]
        [Authorize(Roles = "Advisor")]
        public async Task<IActionResult> GetByMe()
        {
            try
            {
                var result = await _advisorBankAccountService.GetMyAsync();
                return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not Found", 404));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Advisor")]
        public async Task<IActionResult> Create([FromBody] CreateAdvisorBankAccountRequest request)
        {
            try
            {
                var result = await _advisorBankAccountService.CreateAsync(request);
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.AdvisorBankAccountId },
                    ApiResponse<object>.SuccessResponse(result, "Advisor bank account created successfully.", 201));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not Found", 404));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<object>.ErrorResponse(ex.Message, "Conflict", 409));
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Advisor")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAdvisorBankAccountRequest request)
        {
            try
            {
                var result = await _advisorBankAccountService.UpdateAsync(id, request);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Advisor bank account updated successfully."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, "Bad Request", 400));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not Found", 404));
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message, "Forbidden", 403));
            }
        }
    }
}
