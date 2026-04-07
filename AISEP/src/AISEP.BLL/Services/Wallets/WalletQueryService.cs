using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using AISEP.DAL.Common;
using AutoMapper;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.BLL.Services.Wallets
{
    public class WalletQueryService : IWalletQueryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper _mapper;

        public WalletQueryService(IUnitOfWork unitOfWork, ISieveProcessor sieveProcessor, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _sieveProcessor = sieveProcessor;
            _mapper = mapper;
        }

        public async Task<WalletSummaryResponse> GetMyWalletAsync(int userId)
        {
            var advisor = await _unitOfWork.Advisors.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Advisor profile not found.");

            var wallet = await _unitOfWork.Wallets.GetByAdvisorIdAsync(advisor.AdvisorId)
                ?? throw new KeyNotFoundException("Wallet not found.");

            var pendingAmount = await _unitOfWork.WithdrawRequests.GetPendingTotalByWalletIdAsync(wallet.WalletId);

            var response = _mapper.Map<WalletSummaryResponse>(wallet);
            response.PendingWithdrawAmount = pendingAmount;
            response.AvailableBalance = wallet.Balance - pendingAmount;
            return response;
        }

        public async Task<PagedResult<WalletTransactionResponse>> GetMyWalletTransactionsAsync(int userId, SieveModel model)
        {
            var advisor = await _unitOfWork.Advisors.GetByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Advisor profile not found.");

            var wallet = await _unitOfWork.Wallets.GetByAdvisorIdAsync(advisor.AdvisorId)
                ?? throw new KeyNotFoundException("Wallet not found.");

            var query = _unitOfWork.WalletTransactions.GetByWalletIdQuery(wallet.WalletId);
            return await PaginationHelper.PaginateAsync(query, model, _sieveProcessor, x => _mapper.Map<WalletTransactionResponse>(x));
        }

        public async Task<PagedResult<AdvisorWalletResponse>> GetAllAdvisorWalletsAsync(SieveModel model)
        {
            var query = _unitOfWork.Wallets.GetAllQuery();
            return await PaginationHelper.PaginateAsync(query, model, _sieveProcessor, x => _mapper.Map<AdvisorWalletResponse>(x));
        }
    }
}
