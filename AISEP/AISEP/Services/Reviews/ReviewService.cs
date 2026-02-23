using AISEP.Common;
using AISEP.DTOs;
using AISEP.Models.Entities;
using AISEP.Services.CurrentUser;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.Services.Reviews
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ISieveProcessor _sieveProcessor;

        public ReviewService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, ISieveProcessor sieveProcessor) { _unitOfWork = unitOfWork; _currentUserService = currentUserService; _sieveProcessor = sieveProcessor; }

        public async Task<ReviewResponseDto?> CreateReviewAsync(ReviewDto dto)
        {
            var userId = _currentUserService.GetUserId();
            var review = new Review
            {
                Id = Guid.NewGuid(),
                AdvisorId = dto.AdvisorId,
                ReviewerId = userId,
                Rating = dto.Rating,
                ReviewContent = dto.ReviewContent,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Reviews.AddAsync(review);
            await _unitOfWork.SaveChangesAsync();

          
            var created = await _unitOfWork.Reviews.GetByIdAsync(review.Id);
            return MapToResponseDto(created);
        }

        public async Task<PagedResultDto<ReviewResponseDto>> GetAllReviewsAsync(SieveModel model)
        {
            var query = _unitOfWork.Reviews.GetReviewQuery();
            return await ApplySieveAndPaginateAsync(query, model);
        }

        public async Task<ReviewResponseDto?> GetReviewByIdAsync(Guid id)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(id);
            return review != null ? MapToResponseDto(review) : null;
        }

        public async Task<PagedResultDto<ReviewResponseDto>> GetReviewsByAdvisorIdAsync(Guid advisorId, SieveModel model)
        {
            var query = _unitOfWork.Reviews.GetReviewQuery()
                .Where(r => r.AdvisorId == advisorId);
            return await ApplySieveAndPaginateAsync(query, model);
        }

        public async Task<PagedResultDto<ReviewResponseDto>> GetMyReviewsAsync(SieveModel model)
        {
            var userId = _currentUserService.GetUserId();
            if (userId == null)
                throw new UnauthorizedAccessException("User not authenticated");

            var query = _unitOfWork.Reviews.GetReviewQuery()
                .Where(r => r.ReviewerId == userId);
            return await ApplySieveAndPaginateAsync(query, model);
        }

        public async Task<bool> DeleteReviewAsync(Guid id)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(id);
            if (review == null) return false;

            var userId = _currentUserService.GetUserId();
            if (userId == null)
                throw new UnauthorizedAccessException("User not authenticated");

            // Chỉ chủ review mới được xóa
            if (review.ReviewerId != userId)
                throw new UnauthorizedAccessException("You can only delete your own review");

            await _unitOfWork.Reviews.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private async Task<PagedResultDto<ReviewResponseDto>> ApplySieveAndPaginateAsync(
         IQueryable<Review> query,
         SieveModel sieveModel)
        {

            var totalCount = await _sieveProcessor
                .Apply(sieveModel, query, applyPagination: false, applySorting: false)
                .CountAsync();


            var items = await _sieveProcessor
                .Apply(sieveModel, query)
                .ToListAsync();

            var page = sieveModel.Page ?? 1;
            var pageSize = sieveModel.PageSize ?? 10;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResultDto<ReviewResponseDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Items = items.Select(MapToResponseDto)
            };
        }
        private ReviewResponseDto MapToResponseDto(Review? review)
        {
            if (review == null)
            {
                throw new ArgumentNullException(nameof(review));
            }

            return new ReviewResponseDto
            {
                Id            = review.Id,
                AdvisorName   = review.Advisor?.User?.UserName ?? "Unknown",
                ReviewerName  = review.Reviewer?.UserName ?? "Unknown",
                Rating        = review.Rating,
                ReviewContent = review.ReviewContent,
                CreatedAt     = review.CreatedAt
            };
        }
       
    }
}
