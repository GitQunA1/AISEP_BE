using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using Sieve.Models;

namespace AISEP.BLL.Services.Payouts
{
    public interface IPayoutService
    {
        Task<PayoutResponse> MarkPaidAsync(int payoutId, int staffUserId, MarkPayoutPaidRequest request);
        Task<PayoutResponse> RejectAsync(int payoutId, int staffUserId, RejectPayoutRequest request);
        Task<PayoutResponse> RequestRetryAsync(int payoutId, int advisorUserId, RequestPayoutRetryRequest request);
        Task<PagedResult<PayoutResponse>> GetAllAsync(SieveModel model);
        Task<PagedResult<PayoutResponse>> GetMineAsync(int advisorUserId, SieveModel model);
    }
}



