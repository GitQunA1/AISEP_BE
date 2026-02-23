using AISEP.DTOs;
using Sieve.Models;

namespace AISEP.Services.Reviews
{
    public interface IReviewService
    {
        Task<ReviewResponseDto?> CreateReviewAsync(ReviewDto dto);
        Task<ReviewResponseDto?> GetReviewByIdAsync(Guid id);
        Task<PagedResultDto<ReviewResponseDto>> GetAllReviewsAsync(SieveModel model);
        Task<PagedResultDto<ReviewResponseDto>> GetReviewsByAdvisorIdAsync(Guid advisorId, SieveModel model);
        Task<PagedResultDto<ReviewResponseDto>> GetMyReviewsAsync(SieveModel model);
        Task<bool> DeleteReviewAsync(Guid id);
    }
}
