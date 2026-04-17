using AISEP.BLL.DTOs.Responses;

namespace AISEP.BLL.Services.Transactions
{
    public interface ITransactionService
    {
        Task<CollectedBookingCommissionSummaryResponse> GetCollectedBookingCommissionSummaryAsync();
    }
}
