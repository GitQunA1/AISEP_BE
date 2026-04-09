using System.Threading.Channels;

namespace AISEP.BLL.Services.BackgroundServices
{
    public class BlockchainOwnershipAssignmentQueue : IBlockchainOwnershipAssignmentQueue
    {
        private readonly Channel<DocumentOwnerAssignmentWorkItem> _queue;

        public BlockchainOwnershipAssignmentQueue()
        {
            _queue = Channel.CreateUnbounded<DocumentOwnerAssignmentWorkItem>(
                new UnboundedChannelOptions
                {
                    SingleReader = true,
                    SingleWriter = false
                });
        }

        public async ValueTask QueueAsync(
            DocumentOwnerAssignmentWorkItem workItem,
            CancellationToken cancellationToken = default)
        {
            await _queue.Writer.WriteAsync(workItem, cancellationToken);
        }

        public async ValueTask<DocumentOwnerAssignmentWorkItem> DequeueAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
