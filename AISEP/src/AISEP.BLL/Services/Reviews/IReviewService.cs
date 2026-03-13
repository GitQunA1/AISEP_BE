using AISEP.BLL.Helpers;
using AISEP.DAL.Common;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using Sieve.Models;

namespace AISEP.BLL.Services.Reviews
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
