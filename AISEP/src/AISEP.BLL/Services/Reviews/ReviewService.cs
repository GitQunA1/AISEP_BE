using AISEP.BLL.Helpers;
using AISEP.DAL.Common;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.DAL.Entities;
using AISEP.BLL.Services.Users;
using AISEP.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.BLL.Services.Reviews
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
            var booking = await _unitOfWork.Bookings.GetByIdAsync(dto.BookingId)
                ?? throw new KeyNotFoundException("Booking not found.");

            if (booking.CustomerId != userId)
                throw new UnauthorizedAccessException("You can only review your own completed booking.");

            if (booking.Status != BookingStatus.Completed)
                throw new InvalidOperationException("Only completed bookings can be reviewed.");

            if (booking.Review is not null)
                throw new InvalidOperationException("This booking has already been reviewed.");

            var review = new Review
            {
                BookingId = booking.BookingId,
                ReviewerId = userId,
                Rating = dto.Rating,
                ReviewContent = dto.ReviewContent?.Trim(),
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Reviews.AddAsync(review);

            await _unitOfWork.SaveChangesAsync();

            await RefreshAdvisorRatingAsync(booking.AdvisorId);

            var created = await _unitOfWork.Reviews.GetByIdAsync(review.ReviewId);
            return MapToResponseDto(created);
        }

        public async Task<ReviewResponse?> UpdateReviewAsync(int id, UpdateReviewRequest dto)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(id);
            if (review is null)
            {
                return null;
            }

            var userId = _currentUserService.GetUserId();
            if (review.ReviewerId != userId)
                throw new UnauthorizedAccessException("You can only update your own review.");

            review.Rating = dto.Rating;
            review.ReviewContent = dto.ReviewContent?.Trim();

            await _unitOfWork.SaveChangesAsync();
            await RefreshAdvisorRatingAsync(review.Booking.AdvisorId);

            var updated = await _unitOfWork.Reviews.GetByIdAsync(id);
            return MapToResponseDto(updated);
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
                .Where(r => r.Booking.AdvisorId == advisorId);
            return await PaginationHelper.PaginateAsync(query, model, _sieveProcessor, MapToResponseDto);
        }

        public async Task<PagedResult<ReviewResponse>> GetMyReviewsAsync(SieveModel model)
        {
            var userId = _currentUserService.GetUserId();

            var query = _unitOfWork.Reviews.GetReviewQuery()
                .Where(r => r.ReviewerId == userId);
            return await PaginationHelper.PaginateAsync(query, model, _sieveProcessor, MapToResponseDto);
        }

        private async Task RefreshAdvisorRatingAsync(int advisorId)
        {
            var advisor = await _unitOfWork.Advisors.GetByIdAsync(advisorId);
            if (advisor is null)
            {
                return;
            }

            advisor.Rating = await _unitOfWork.Reviews.GetAverageRatingByAdvisorIdAsync(advisorId);
            _unitOfWork.Advisors.Update(advisor);
            await _unitOfWork.SaveChangesAsync();
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
                BookingId     = review.BookingId,
                AdvisorName   = review.Booking?.Advisor?.User?.UserName ?? "Unknown",
                ReviewerId    = review.ReviewerId,
                ReviewerName  = review.Reviewer?.UserName ?? "Unknown",
                Rating        = review.Rating,
                ReviewContent = review.ReviewContent,
                CreatedAt     = review.CreatedAt
            };
        }
       
    }
}

