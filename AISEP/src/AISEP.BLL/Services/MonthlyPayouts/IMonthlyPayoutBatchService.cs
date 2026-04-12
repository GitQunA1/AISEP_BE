using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using Sieve.Models;

namespace AISEP.BLL.Services.MonthlyPayouts
{
    public interface IMonthlyPayoutBatchService
    {
        Task<List<MonthlyPayoutResponse>> GenerateAsync(GenerateMonthlyPayoutRequest request);
        Task<PagedResult<MonthlyPayoutBatchResponse>> GetBatchesAsync(SieveModel model);
        Task<MonthlyPayoutBatchResponse?> GetBatchByIdAsync(int batchId);
        Task<PagedResult<MonthlyPayoutResponse>> GetItemsByBatchIdAsync(int batchId, SieveModel model);
        Task RecalculateAsync(int batchId);
    }
}
