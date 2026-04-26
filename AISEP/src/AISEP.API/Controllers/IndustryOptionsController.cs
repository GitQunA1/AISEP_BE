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
        public async Task<IActionResult> GetAll([FromQuery] SieveModel model, [FromQuery] bool includeInactive = false)
        {
            var result = await _industryOptionService.GetAllAsync(model, includeInactive);
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

      
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateIndustryOptionRequest request)
        {
            try
            {
                var result = await _industryOptionService.UpdateAsync(id, request);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Industry option updated successfully."));
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
