using AISEP.DAL.Enums;

namespace AISEP.BLL.Services.Wallets
{
    public interface IWalletService
    {
        Task SyncWithAdvisorApprovalStatusAsync(int advisorId, ApprovalStatus approvalStatus, bool createWalletIfApproved);
    }
}
