namespace AISEP.BLL.Services.BackgroundServices
{
    public interface IBlockchainOwnershipAssignmentQueue
    {
        ValueTask QueueAsync(
            DocumentOwnerAssignmentWorkItem workItem,
            CancellationToken cancellationToken = default);

        ValueTask<DocumentOwnerAssignmentWorkItem> DequeueAsync(CancellationToken cancellationToken);
    }
}
