using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.ConnectionRequests
{
    public interface IConnectionRequestRepository
    {
        Task<ConnectionRequest?> GetByIdAsync(int requestId);
        Task<ConnectionRequest?> GetByInvestorAndProjectAsync(int investorId, int projectId);
        Task<bool> ExistsAcceptedAsync(int investorId, int projectId);
        Task AddAsync(ConnectionRequest request);
        void Update(ConnectionRequest request);
    }
}
