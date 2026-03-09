using AISEP.Common;
using AISEP.DTOs.Requests;
using AISEP.DTOs.Responses;
using AISEP.Models.Entities;
using AISEP.Services.Users;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.Services.Reviews
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _currentUserService;
        private readonly ISieveProcessor _sieveProcessor;

        public ReviewService(IUnitOfWork unitOfWork, IUserService currentUserService, ISieveProcessor sieveProcessor)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _sieveProcessor = sieveProcessor;
        }

        public async Task<ReviewResponse?> CreateReviewAsync(CreateReviewRequest dto)
        {
            var userId = _currentUserService.GetUserId();
            var review = new Review
            {
                AdvisorId = dto.AdvisorId,
                ReviewerId = userId,
                Rating = dto.Rating,
                ReviewContent = dto.ReviewContent,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Reviews.AddAsync(review);
            await _unitOfWork.SaveChangesAsync();


            var created = await _unitOfWork.Reviews.GetByIdAsync(review.ReviewId);
            return MapToResponseDto(created);
        }

        public async Task<PagedResult<ReviewResponse>> GetAllReviewsAsync(SieveModel model)
        {
            var query = _unitOfWork.Reviews.GetReviewQuery();
            return await PaginationHelper.PaginateAsync(query, model, _sieveProcessor, MapToResponseDto);
        }

        public async Task<ReviewResponse?> GetReviewByIdAsync(int id)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(id);
            return review != null ? MapToResponseDto(review) : null;
        }

        public async Task<PagedResult<ReviewResponse>> GetReviewsByAdvisorIdAsync(int advisorId, SieveModel model)
        {
            var query = _unitOfWork.Reviews.GetReviewQuery()
                .Where(r => r.AdvisorId == advisorId);
            return await PaginationHelper.PaginateAsync(query, model, _sieveProcessor, MapToResponseDto);
        }

        public async Task<PagedResult<ReviewResponse>> GetMyReviewsAsync(SieveModel model)
        {
            var userId = _currentUserService.GetUserId();

            var query = _unitOfWork.Reviews.GetReviewQuery()
                .Where(r => r.ReviewerId == userId);
            return await PaginationHelper.PaginateAsync(query, model, _sieveProcessor, MapToResponseDto);
        }

        public async Task<bool> DeleteReviewAsync(int id)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(id);
            if (review == null) return false;

            var userId = _currentUserService.GetUserId();

            // Ch? ch? review m?i du?c xóa
            if (review.ReviewerId != userId)
                throw new UnauthorizedAccessException("You can only delete your own review");

            await _unitOfWork.Reviews.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private ReviewResponse MapToResponseDto(Review? review)
        {
            if (review == null)
            {
                throw new ArgumentNullException(nameof(review));
            }

            return new ReviewResponse
            {
                Id            = review.ReviewId,
                AdvisorName   = review.Advisor?.User?.UserName ?? "Unknown",
                ReviewerName  = review.Reviewer?.UserName ?? "Unknown",
                Rating        = review.Rating,
                ReviewContent = review.ReviewContent,
                CreatedAt     = review.CreatedAt
            };
        }
       
    }
}

