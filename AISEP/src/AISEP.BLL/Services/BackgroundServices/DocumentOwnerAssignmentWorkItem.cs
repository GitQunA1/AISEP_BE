namespace AISEP.BLL.Services.BackgroundServices
{
    public sealed record DocumentOwnerAssignmentWorkItem(
        int DealId,
        int ProjectId,
        string DocumentHash,
        string InvestorWallet,
        int InvestorUserId);
}
