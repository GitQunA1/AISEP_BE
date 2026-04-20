using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using Sieve.Models;

namespace AISEP.BLL.Services.Transactions
{
    public interface ITransactionService
    {
        Task<CollectedBookingCommissionSummaryResponse> GetCollectedBookingCommissionSummaryAsync();
        Task<PagedResult<AdminTransactionResponse>> GetAllForAdminAsync(SieveModel model);
    }
}
