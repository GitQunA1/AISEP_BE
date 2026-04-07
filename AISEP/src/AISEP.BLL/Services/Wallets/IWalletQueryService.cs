using AISEP.BLL.DTOs.Responses;
using Sieve.Models;

namespace AISEP.BLL.Services.Wallets
{
    public interface IWalletQueryService
    {
        Task<WalletSummaryResponse> GetMyWalletAsync(int userId);
        Task<PagedResult<WalletTransactionResponse>> GetMyWalletTransactionsAsync(int userId, SieveModel model);
        Task<PagedResult<AdvisorWalletResponse>> GetAllAdvisorWalletsAsync(SieveModel model);
    }
}
