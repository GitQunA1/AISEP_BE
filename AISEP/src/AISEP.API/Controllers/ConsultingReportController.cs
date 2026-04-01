using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.ConsultingReports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConsultingReportController : ControllerBase
    {
        private readonly IConsultingReportService _consultingReportService;

        public ConsultingReportController(IConsultingReportService consultingReportService)
        {
            _consultingReportService = consultingReportService;
        }

        [HttpPost]
        [Authorize(Roles = "Advisor")]
        public async Task<IActionResult> Create([FromBody] CreateConsultingReportRequest request)
        {
            var result = await _consultingReportService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.ConsultingReportId },
                ApiResponse<object>.SuccessResponse(result, "Consulting report created successfully.", 201));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _consultingReportService.GetByIdAsync(id);
            if (result is null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Consulting report not found.", "Not found", 404));
            }

            return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
        }

        [HttpGet("booking/{bookingId:int}")]
        public async Task<IActionResult> GetByBooking(int bookingId)
        {
            var result = await _consultingReportService.GetByBookingIdAsync(bookingId);
            if (result is null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Consulting report not found.", "Not found", 404));
            }

            return Ok(ApiResponse<object>.SuccessResponse(result, "Success"));
        }

        [HttpPatch("{id:int}/startup-approve")]
        [Authorize(Roles = "Startup,Investor")]
        public async Task<IActionResult> Approve(int id)
        {
            var result = await _consultingReportService.ApproveAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Report approved successfully."));
        }

        [HttpPatch("{id:int}/startup-request-revision")]
        [Authorize(Roles = "Startup,Investor")]
        public async Task<IActionResult> RequestRevision(int id, [FromBody] RequestReportRevisionRequest request)
        {
            var result = await _consultingReportService.RequestRevisionAsync(id, request.Reason);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Revision request submitted."));
        }
    }
}
