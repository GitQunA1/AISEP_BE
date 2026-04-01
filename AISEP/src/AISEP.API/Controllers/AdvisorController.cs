using AISEP.BLL.Helpers;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Services.Advisors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class AdvisorController : ControllerBase
    {
        private readonly IAdvisorService _advisorService;

        public AdvisorController(IAdvisorService advisorService)
        {
            _advisorService = advisorService;
        }

       
        [HttpGet]
     
        public async Task<IActionResult> GetAll([FromQuery] SieveModel model)
        {
            var result = await _advisorService.GetAllAsync(model);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
        }

       
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var advisor = await _advisorService.GetByIdAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse(advisor, "Success"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not Found", 404));
            }
        }

       
        [HttpGet("me")]
        [Authorize(Roles ="Advisor")]
        public async Task<IActionResult> GetMyProfile()
        {
            try
            {
                var advisor = await _advisorService.GetMyProfileAsync();
                return Ok(ApiResponse<object>.SuccessResponse(advisor, "Success"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, "Not Found", 404));
            }
        }

  
        [HttpPost]
        [Authorize(Roles = "Advisor")]
        public async Task<IActionResult> Create([FromForm] CreateAdvisorRequest dto)
        {
            try
            {
                var data = await _advisorService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = data.AdvisorId },
                    ApiResponse<object>.SuccessResponse(data, "Advisor created successfully.", 201));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<object>.ErrorResponse(ex.Message, "Conflict", 409));
            }
        }


        [HttpPut("{id:int}")]
        [Authorize(Roles = "Advisor")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateAdvisorRequest dto)
        {
            try
            {
                var data = await _advisorService.UpdateAsync(id, dto);
                return Ok(ApiResponse<object>.SuccessResponse(data, "Advisor updated successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Advisor profile not found.", "Not found", 404));
            }
        }


        [HttpPatch("{advisorId:int}/approve")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> ApproveAdvisor(int advisorId)
        {
            try
            {
                await _advisorService.ApproveAdvisorAsync(advisorId);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Advisor approved successfully."));
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

        [HttpPatch("{advisorId:int}/reject")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> RejectAdvisor(int advisorId, [FromBody] RejectAdvisorRequest dto)
        {
            try
            {
                await _advisorService.RejectAdvisorAsync(advisorId, dto.Reason);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Advisor rejected successfully."));
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

        //[HttpDelete("{id:int}")]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    var deleted = await _advisorService.DeleteAsync(id);
        //    if (!deleted)
        //        return NotFound(ApiResponse<object>.ErrorResponse("Advisor not found.", "Not found", 404));

        //    return Ok(ApiResponse<object>.SuccessResponse(null!, "Advisor deleted successfully."));
        //}
    }
}
