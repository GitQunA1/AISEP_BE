using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.ConnectionRequests
{
    public interface IConnectionRequestRepository
    {
        Task<ConnectionRequest?> GetByIdAsync(int requestId);
        Task<ConnectionRequest?> GetByInvestorAndProjectAsync(int investorId, int projectId);
        IQueryable<ConnectionRequest> GetByInvestorQuery(int investorId);
        IQueryable<ConnectionRequest> GetByStartupQuery(int startupId);
        Task<bool> ExistsAcceptedAsync(int investorId, int projectId);
        Task AddAsync(ConnectionRequest request);
        void Update(ConnectionRequest request);
    }
}
