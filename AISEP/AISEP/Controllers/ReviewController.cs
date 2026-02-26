using AISEP.Models.DTOs;
using AISEP.Services.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace AISEP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

 
        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] ReviewDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _reviewService.CreateReviewAsync(dto);
                return CreatedAtAction(nameof(GetReviewById), new { id = result!.Id }, result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetReviewById(int id)
        {
            var review = await _reviewService.GetReviewByIdAsync(id);
            if (review == null)
                return NotFound(new { message = "Review not found" });

            return Ok(review);
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllReviews([FromQuery] SieveModel model)
        {
            var reviews = await _reviewService.GetAllReviewsAsync(model);
            return Ok(reviews);
        }


        [HttpGet("advisor/{advisorId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetReviewsByAdvisor(int advisorId, [FromQuery] SieveModel model)
        {
            var reviews = await _reviewService.GetReviewsByAdvisorIdAsync(advisorId, model);
            return Ok(reviews);
        }

        [HttpGet("my-reviews")]
        public async Task<IActionResult> GetMyReviews([FromQuery] SieveModel model)
        {
            try
            {
                var reviews = await _reviewService.GetMyReviewsAsync(model);
                return Ok(reviews);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }


        //[HttpDelete("{id:int}")]
        //public async Task<IActionResult> DeleteReview(int id)
        //{
        //    try
        //    {
        //        var result = await _reviewService.DeleteReviewAsync(id);
        //        if (!result)
        //            return NotFound(new { message = "Review not found" });

        //        return NoContent();
        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        return Unauthorized(new { message = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}
    }
}
