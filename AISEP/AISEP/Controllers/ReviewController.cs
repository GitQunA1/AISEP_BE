using AISEP.Common;
using AISEP.DTOs;
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
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse.Fail("Invalid request data."));

            var result = await _reviewService.CreateReviewAsync(dto);
            if (result is null)
                return BadRequest(ApiResponse.Fail("Could not create review."));

            return CreatedAtAction(nameof(GetReviewById), new { id = result.Id },
                ApiResponse.Success(result));
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetReviewById(int id)
        {
            var review = await _reviewService.GetReviewByIdAsync(id);
            if (review is null)
                return NotFound(ApiResponse.Fail("Review not found."));

            return Ok(ApiResponse.Success(review));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllReviews([FromQuery] SieveModel model)
        {
            var reviews = await _reviewService.GetAllReviewsAsync(model);
            return Ok(ApiResponse.Success(reviews));
        }

        [HttpGet("advisor/{advisorId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetReviewsByAdvisor(int advisorId, [FromQuery] SieveModel model)
        {
            var reviews = await _reviewService.GetReviewsByAdvisorIdAsync(advisorId, model);
            return Ok(ApiResponse.Success(reviews));
        }

        [HttpGet("my-reviews")]
        public async Task<IActionResult> GetMyReviews([FromQuery] SieveModel model)
        {
            var reviews = await _reviewService.GetMyReviewsAsync(model);
            return Ok(ApiResponse.Success(reviews));
        }
    }
}
