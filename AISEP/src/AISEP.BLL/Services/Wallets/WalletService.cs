using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;

namespace AISEP.BLL.Services.Wallets
{
    public class WalletService : IWalletService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WalletService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task SyncWithAdvisorApprovalStatusAsync(int advisorId, ApprovalStatus approvalStatus, bool createWalletIfApproved)
        {
            var wallet = await _unitOfWork.Wallets.GetByAdvisorIdAsync(advisorId);

            if (approvalStatus == ApprovalStatus.Approved)
            {
                if (wallet is null)
                {
                    if (!createWalletIfApproved)
                    {
                        return;
                    }

                    await _unitOfWork.Wallets.AddAsync(new Wallet
                    {
                        AdvisorId = advisorId,
                        Balance = 0m,
                        Currency = "VND",
                        IsActive = true
                    });
                    return;
                }

                wallet.IsActive = true;
                _unitOfWork.Wallets.Update(wallet);
                return;
            }

            if (wallet is not null)
            {
                wallet.IsActive = false;
                _unitOfWork.Wallets.Update(wallet);
            }
        }
    }
}
