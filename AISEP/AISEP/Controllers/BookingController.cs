using AISEP.DTOs;
using AISEP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using System.Security.Claims;

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
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

             
                var booking = await _bookingService.CreateBookingAsync(dto);
                return CreatedAtAction(nameof(GetBookingById), new { id = booking!.Id }, booking);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookingById(Guid id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
            {
                return NotFound(new { message = "Booking not found" });
            }

            return Ok(booking);
        }

        
        [HttpGet]
        public async Task<IActionResult> GetAllBookings([FromQuery] SieveModel model)
        {
            var bookings = await _bookingService.GetAllBookingsAsync(model);
            return Ok(bookings);
        }

        //[HttpGet("advisor/{advisorId}")]
        //public async Task<IActionResult> GetBookingsByAdvisorId(Guid advisorId, [FromQuery] SieveModel sieveModel)
        //{
        //    var bookings = await _bookingService.GetBookingsByAdvisorIdAsync(advisorId, sieveModel);
        //    return Ok(bookings);
        //}

      
        //[HttpGet("customer/{customerId}")]
        //public async Task<IActionResult> GetBookingsByCustomerId(Guid customerId, [FromQuery] SieveModel sieveModel)
        //{
        //    var bookings = await _bookingService.GetBookingsByCustomerIdAsync(customerId, sieveModel);
        //    return Ok(bookings);
        //}

     

       
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(Guid id)
        {
            var result = await _bookingService.DeleteBookingAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Booking not found" });
            }

            return NoContent();
        }
    }
}
