using AISEP.Common;
using AISEP.DTOs;
using AISEP.Services.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace AISEP.Controllers
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
        public async Task<IActionResult> CreateBooking([FromBody] BookingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse.Fail("Invalid request data."));

            var booking = await _bookingService.CreateBookingAsync(dto);
            if (booking is null)
                return BadRequest(ApiResponse.Fail("Could not create booking."));

            return CreatedAtAction(nameof(GetBookingById), new { id = booking.Id },
                ApiResponse.Success(booking));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookingById(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking is null)
                return NotFound(ApiResponse.Fail("Booking not found."));

            return Ok(ApiResponse.Success(booking));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBookings([FromQuery] SieveModel model)
        {
            var bookings = await _bookingService.GetAllBookingsAsync(model);
            return Ok(ApiResponse.Success(bookings));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var result = await _bookingService.DeleteBookingAsync(id);
            if (!result)
                return NotFound(ApiResponse.Fail("Booking not found."));

            return Ok(ApiResponse.Success());
        }
    }
}
