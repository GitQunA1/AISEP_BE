using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using Sieve.Models;

namespace AISEP.BLL.Services.Payouts
{
    public interface IPayoutGroupService
    {
        Task<List<PayoutResponse>> GenerateAsync(GeneratePayoutGroupRequest request);
        Task<PagedResult<PayoutGroupResponse>> GetBatchesAsync(SieveModel model);
        Task<PayoutGroupResponse?> GetBatchByIdAsync(int batchId);
        Task<PagedResult<PayoutResponse>> GetItemsByBatchIdAsync(int batchId, SieveModel model);
        Task RecalculateAsync(int batchId);
    }
}



