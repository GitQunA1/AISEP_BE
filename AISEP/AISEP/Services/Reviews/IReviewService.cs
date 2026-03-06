using AISEP.DTOs;
using AISEP.DTOs.Requests;
using AISEP.DTOs.Responses;
using Sieve.Models;

namespace AISEP.Services.Reviews
{
    public interface IReviewService
    {
        Task<ReviewResponse?> CreateReviewAsync(CreateReviewRequest dto);
        Task<ReviewResponse?> GetReviewByIdAsync(int id);
        Task<PagedResult<ReviewResponse>> GetAllReviewsAsync(SieveModel model);
        Task<PagedResult<ReviewResponse>> GetReviewsByAdvisorIdAsync(int advisorId, SieveModel model);
        Task<PagedResult<ReviewResponse>> GetMyReviewsAsync(SieveModel model);
        Task<bool> DeleteReviewAsync(int id);
    }
}
