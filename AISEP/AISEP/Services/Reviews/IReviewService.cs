using AISEP.DTOs;
using AISEP.Models.DTOs;
using Sieve.Models;

namespace AISEP.Services.Reviews
{
    public interface IReviewService
    {
        Task<ReviewResponseDto?> CreateReviewAsync(ReviewDto dto);
        Task<ReviewResponseDto?> GetReviewByIdAsync(int id);
        Task<PagedResultDto<ReviewResponseDto>> GetAllReviewsAsync(SieveModel model);
        Task<PagedResultDto<ReviewResponseDto>> GetReviewsByAdvisorIdAsync(int advisorId, SieveModel model);
        Task<PagedResultDto<ReviewResponseDto>> GetMyReviewsAsync(SieveModel model);
        Task<bool> DeleteReviewAsync(int id);
    }
}
