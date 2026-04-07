using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using AISEP.DAL.Common;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.BLL.Services.Wallets
{
    public class WalletQueryService : IWalletQueryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;

        public WalletQueryService(IUnitOfWork unitOfWork, ISieveProcessor sieveProcessor)
        {
            _unitOfWork = unitOfWork;
            _sieveProcessor = sieveProcessor;
        }

        public async Task<WalletSummaryResponse> GetMyWalletAsync(int userId)
        {
            var advisor = await _unitOfWork.Advisors.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Advisor profile not found.");

            var wallet = await _unitOfWork.Wallets.GetByAdvisorIdAsync(advisor.AdvisorId)
                ?? throw new KeyNotFoundException("Wallet not found.");

            var pendingAmount = await _unitOfWork.WithdrawRequests.GetPendingTotalByWalletIdAsync(wallet.WalletId);

            return new WalletSummaryResponse
            {
                WalletId = wallet.WalletId,
                AdvisorId = advisor.AdvisorId,
                Balance = wallet.Balance,
                Currency = wallet.Currency,
                IsActive = wallet.IsActive,
                PendingWithdrawAmount = pendingAmount,
                AvailableBalance = wallet.Balance - pendingAmount
            };
        }

        public async Task<PagedResult<WalletTransactionResponse>> GetMyWalletTransactionsAsync(int userId, SieveModel model)
        {
            var advisor = await _unitOfWork.Advisors.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Advisor profile not found.");

            var wallet = await _unitOfWork.Wallets.GetByAdvisorIdAsync(advisor.AdvisorId)
                ?? throw new KeyNotFoundException("Wallet not found.");

            var query = _unitOfWork.WalletTransactions.GetByWalletIdQuery(wallet.WalletId);
            return await PaginationHelper.PaginateAsync(query, model, _sieveProcessor, x => new WalletTransactionResponse
            {
                WalletTransactionId = x.WalletTransactionId,
                WalletId = x.WalletId,
                WithdrawRequestId = x.WithdrawRequestId,
                Amount = x.Amount,
                Type = x.Type.ToString(),
                Status = x.Status.ToString(),
                CreatedAt = x.CreatedAt
            });
        }
    }
}
