using AISEP.BLL.Common;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Services.Bookings;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost]
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

        [HttpGet("advisor/{advisorId:int}")]
        public async Task<IActionResult> GetByAdvisor(int advisorId, [FromQuery] SieveModel model)
        {
            var bookings = await _bookingService.GetBookingsByAdvisorIdAsync(advisorId, model);
            return Ok(ApiResponse<object>.SuccessResponse(bookings, "Success"));
        }

        [HttpGet("customer/{customerId:int}")]
        public async Task<IActionResult> GetByCustomer(int customerId, [FromQuery] SieveModel model)
        {
            var bookings = await _bookingService.GetBookingsByCustomerIdAsync(customerId, model);
            return Ok(ApiResponse<object>.SuccessResponse(bookings, "Success"));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var result = await _bookingService.DeleteBookingAsync(id);
            if (!result)
                return NotFound(ApiResponse<object>.ErrorResponse("Booking not found.", "Not found", 404));

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Deleted successfully"));
        }
    }
}
