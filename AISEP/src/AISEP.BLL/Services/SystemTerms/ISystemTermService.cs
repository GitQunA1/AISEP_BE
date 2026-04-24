using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using Sieve.Models;

namespace AISEP.BLL.Services.SystemTerms
{
    public interface ISystemTermService
    {
        Task<SystemTermResponse> PublishAsync(CreateSystemTermRequest request);
        Task<SystemTermResponse> GetActiveAsync();
        Task<PagedResult<SystemTermResponse>> GetHistoryAsync(SieveModel model);
    }
}
