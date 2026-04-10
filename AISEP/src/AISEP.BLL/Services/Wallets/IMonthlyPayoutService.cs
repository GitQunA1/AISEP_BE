using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using Sieve.Models;

namespace AISEP.BLL.Services.Wallets
{
    public interface IMonthlyPayoutService
    {
        Task<List<MonthlyPayoutResponse>> GenerateAsync(GenerateMonthlyPayoutRequest request);
        Task<MonthlyPayoutResponse> MarkPaidAsync(int monthlyPayoutId, int staffUserId, MarkMonthlyPayoutPaidRequest request);
        Task<PagedResult<MonthlyPayoutResponse>> GetAllAsync(SieveModel model);
        Task<PagedResult<MonthlyPayoutResponse>> GetMineAsync(int advisorUserId, SieveModel model);
    }
}
