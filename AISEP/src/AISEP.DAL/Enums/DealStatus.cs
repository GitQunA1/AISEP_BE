namespace AISEP.DAL.Enums
{
    public enum DealStatus
    {
        PendingCounterpartyConfirmation,
        PendingStaffApproval,
        RequireReupload,
        ProcessingBlockchain,
        Completed,
        Canceled,
        BlockchainFailed
    }
}
