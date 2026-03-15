using AISEP.BLL.Helpers;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Services.Advisors;
using AISEP.BLL.Services.Users;
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
        private readonly IUserService    _userService;

        public AdvisorController(IAdvisorService advisorService, IUserService userService)
        {
            _advisorService = advisorService;
            _userService    = userService;
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
            var advisor = await _advisorService.GetByIdAsync(id);
            if (advisor is null)
                return NotFound(ApiResponse<object>.ErrorResponse("Advisor not found.", "Not found", 404));

            return Ok(ApiResponse<object>.SuccessResponse(advisor, "Success"));
        }

       
        [HttpGet("me")]
        [Authorize(Roles ="Advisor")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId  = _userService.GetUserId();
            var advisor = await _advisorService.GetMyProfileAsync(userId);
            if (advisor is null)
                return NotFound(ApiResponse<object>.ErrorResponse("Advisor profile not found.", "Not found", 404));

            return Ok(ApiResponse<object>.SuccessResponse(advisor, "Success"));
        }

  
        [HttpPost]
        [Authorize(Roles = "Advisor")]
        public async Task<IActionResult> Create([FromForm] CreateAdvisorRequest dto)
        {
            //var userId = _userService.GetUserId();
            var data   = await _advisorService.CreateAsync( dto);

            if (data is null)
                return Conflict(ApiResponse<object>.ErrorResponse("Advisor profile already exists.", "Conflict", 409));

            return CreatedAtAction(nameof(GetById), new { id = data.AdvisorId },
                ApiResponse<object>.SuccessResponse(data, "Advisor created successfully.", 201));
        }


        [HttpPut("{id:int}")]
        [Authorize(Roles = "Advisor")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateAdvisorRequest dto)
        {
            
            var data   = await _advisorService.UpdateAsync(id, dto);

            if (data is null)
                return NotFound(ApiResponse<object>.ErrorResponse("Advisor profile not found.", "Not found", 404));

            return Ok(ApiResponse<object>.SuccessResponse(data, "Advisor updated successfully."));
        }

  
        [HttpPatch("{advisorId:int}/approve")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> ApproveAdvisor(int advisorId)
        {
            await _advisorService.ApproveAdvisorAsync(advisorId);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Advisor approved successfully."));
        }

        [HttpPatch("{advisorId:int}/reject")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> RejectAdvisor(int advisorId, [FromBody] RejectRequest dto)
        {
            
            await _advisorService.RejectAdvisorAsync(advisorId, dto.Reason);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Advisor rejected successfully."));
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
