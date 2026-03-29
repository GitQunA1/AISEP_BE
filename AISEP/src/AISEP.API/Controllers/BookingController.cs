using AISEP.BLL.Helpers;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Services.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost]
        [Authorize(Roles = "Investor,Startup")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest dto)
        {
            var booking = await _bookingService.CreateBookingAsync(dto);
            if (booking is null)
                return BadRequest(ApiResponse<object>.ErrorResponse("Could not create booking.", "Failed"));

            return CreatedAtAction(nameof(GetBookingById), new { id = booking.Id },
                ApiResponse<object>.SuccessResponse(booking, "Booking created successfully", 201));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetBookingById(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking is null)
                return NotFound(ApiResponse<object>.ErrorResponse("Booking not found.", "Not found", 404));

            return Ok(ApiResponse<object>.SuccessResponse(booking, "Success"));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBookings([FromQuery] SieveModel model)
        {
            var bookings = await _bookingService.GetAllBookingsAsync(model);
            return Ok(ApiResponse<object>.SuccessResponse(bookings, "Success"));
        }
        //Trả danh sách project để dropdown booking.
        [HttpGet("project-options")]
        [Authorize(Roles = "Investor,Startup")]
        public async Task<IActionResult> GetBookingProjectOptions()
        {
            var projects = await _bookingService.GetBookingProjectOptionsAsync();
            return Ok(ApiResponse<object>.SuccessResponse(projects, "Success"));
        }
        //Trả danh sách advisor để dropdown booking, dựa trên project đã chọn.
        [HttpGet("advisor-options")]
        [Authorize(Roles = "Investor,Startup")]
        public async Task<IActionResult> GetBookingAdvisorOptions([FromQuery] int projectId)
        {
            var advisors = await _bookingService.GetBookingAdvisorOptionsAsync(projectId);
            return Ok(ApiResponse<object>.SuccessResponse(advisors, "Success"));
        }

        //Trả danh sách advisor thay thế cho booking đã reject/noresponse
        [HttpGet("{id:int}/replacement-advisor-options")]
        [Authorize(Roles = "Investor,Startup")]
        public async Task<IActionResult> GetReplacementAdvisorOptions(int id)
        {
            var advisors = await _bookingService.GetReplacementAdvisorOptionsAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(advisors, "Success"));
        }

        //[HttpGet("advisor/{advisorId:int}")]
        //public async Task<IActionResult> GetByAdvisor(int advisorId, [FromQuery] SieveModel model)
        //{
        //    var bookings = await _bookingService.GetBookingsByAdvisorIdAsync(advisorId, model);
        //    return Ok(ApiResponse<object>.SuccessResponse(bookings, "Success"));
        //}

        //[HttpGet("customer/{customerId:int}")]
        //public async Task<IActionResult> GetByCustomer(int customerId, [FromQuery] SieveModel model)
        //{
        //    var bookings = await _bookingService.GetBookingsByCustomerIdAsync(customerId, model);
        //    return Ok(ApiResponse<object>.SuccessResponse(bookings, "Success"));
        //}

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var result = await _bookingService.DeleteBookingAsync(id);
            if (!result)
                return NotFound(ApiResponse<object>.ErrorResponse("Booking not found.", "Not found", 404));

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Deleted successfully"));
        }

        [HttpPatch("{id:int}/approve")]
        [Authorize(Roles = "Advisor")]
        public async Task<IActionResult> ApproveBooking(int id)
        {
            var booking = await _bookingService.ApproveBookingAsync(id);
            if (booking is null)
                return NotFound(ApiResponse<object>.ErrorResponse("Booking not found.", "Not found", 404));

            return Ok(ApiResponse<object>.SuccessResponse(booking, "Booking approved successfully."));
        }

        [HttpPatch("{id:int}/reject")]
        [Authorize(Roles = "Advisor")]
        public async Task<IActionResult> RejectBooking(int id, [FromBody] RejectBookingRequest request)
        {
            var booking = await _bookingService.RejectBookingAsync(id, request.Reason);
            if (booking is null)
                return NotFound(ApiResponse<object>.ErrorResponse("Booking not found.", "Not found", 404));

            return Ok(ApiResponse<object>.SuccessResponse(booking, "Booking rejected successfully."));
        }
    }
}
