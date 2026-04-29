using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.StageOptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/stage-options")]
    public class StageOptionsController : ControllerBase
    {
        private readonly IStageOptionService _stageOptionService;

        public StageOptionsController(IStageOptionService stageOptionService)
        {
            _stageOptionService = stageOptionService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] SieveModel model)
        {
            var result = await _stageOptionService.GetAllAsync(model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Stage options retrieved successfully."));
        }

        [HttpPost]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateStageOptionRequest request)
        {
            try
            {
                var result = await _stageOptionService.CreateAsync(request);
                return StatusCode(201, ApiResponse<object>.SuccessResponse(result, "Stage option created successfully.", 201));
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
                var result = await _stageOptionService.SetActiveAsync(id, true);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Stage option activated successfully."));
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
                var result = await _stageOptionService.SetActiveAsync(id, false);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Stage option deactivated successfully."));
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
