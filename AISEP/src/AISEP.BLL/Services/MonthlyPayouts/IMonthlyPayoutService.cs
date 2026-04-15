using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using Sieve.Models;

namespace AISEP.BLL.Services.MonthlyPayouts
{
    public interface IMonthlyPayoutService
    {
        Task<MonthlyPayoutResponse> MarkPaidAsync(int monthlyPayoutId, int staffUserId, MarkMonthlyPayoutPaidRequest request);
        Task<MonthlyPayoutResponse> RejectAsync(int monthlyPayoutId, int staffUserId, RejectMonthlyPayoutRequest request);
        Task<MonthlyPayoutResponse> RequestRetryAsync(int monthlyPayoutId, int advisorUserId, RequestMonthlyPayoutRetryRequest request);
        Task<PagedResult<MonthlyPayoutResponse>> GetAllAsync(SieveModel model);
        Task<PagedResult<MonthlyPayoutResponse>> GetMineAsync(int advisorUserId, SieveModel model);
    }
}
