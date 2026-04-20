using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using Sieve.Models;

namespace AISEP.BLL.Services.Payouts
{
    public interface IPayoutGroupService
    {
        Task<List<PayoutResponse>> GenerateAsync(GeneratePayoutGroupRequest request);
        Task<PagedResult<PayoutGroupResponse>> GetGroupsAsync(SieveModel model);
        Task<PayoutGroupResponse?> GetGroupByIdAsync(int groupId);
        Task<PagedResult<PayoutResponse>> GetItemsByGroupIdAsync(int groupId, SieveModel model);
        Task RecalculateAsync(int groupId);
    }
}



