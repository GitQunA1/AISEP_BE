using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.IndustryOptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/industry-options")]
    public class IndustryOptionsController : ControllerBase
    {
        private readonly IIndustryOptionService _industryOptionService;

        public IndustryOptionsController(IIndustryOptionService industryOptionService)
        {
            _industryOptionService = industryOptionService;
        }

       
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] SieveModel model)
        {
            var result = await _industryOptionService.GetAllAsync(model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Industry options retrieved successfully."));
        }

        
        [HttpPost]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateIndustryOptionRequest request)
        {
            try
            {
                var result = await _industryOptionService.CreateAsync(request);
                return StatusCode(201, ApiResponse<object>.SuccessResponse(result, "Industry option created successfully.", 201));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, "Bad Request", 400));
            }
        }

        [HttpPatch("{id:int}/activate")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> Activate(int id)
        {
            try
            {
                var result = await _industryOptionService.SetActiveAsync(id, true);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Industry option activated successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not Found", 404));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, "Bad Request", 400));
            }
        }

        [HttpPatch("{id:int}/deactivate")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                var result = await _industryOptionService.SetActiveAsync(id, false);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Industry option deactivated successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not Found", 404));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, "Bad Request", 400));
            }
        }
    }
}
