using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.ConnectionRequests
{
    public class ConnectionRequestRepository : IConnectionRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public ConnectionRequestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ConnectionRequest?> GetByIdAsync(int requestId)
        {
            return await _context.ConnectionRequests
                .Include(cr => cr.ChatSession)
                .Include(cr => cr.Investor)
                .Include(cr => cr.Project)
                    .ThenInclude(p => p.Startup)
                .FirstOrDefaultAsync(cr => cr.ConnectionRequestId == requestId);
        }

        public async Task<ConnectionRequest?> GetByInvestorAndProjectAsync(int investorId, int projectId)
        {
            return await _context.ConnectionRequests
                .Include(cr => cr.ChatSession)
                .FirstOrDefaultAsync(cr => cr.InvestorId == investorId && cr.ProjectId == projectId);
        }

        public IQueryable<ConnectionRequest> GetByInvestorQuery(int investorId)
        {
            return _context.ConnectionRequests
                .Include(cr => cr.ChatSession)
                .Where(cr => cr.InvestorId == investorId)
                .OrderByDescending(cr => cr.ConnectionRequestId)
                .AsNoTracking();
        }

        public IQueryable<ConnectionRequest> GetByStartupQuery(int startupId)
        {
            return _context.ConnectionRequests
                .Include(cr => cr.ChatSession)
                .Where(cr => cr.Project.StartupId == startupId)
                .OrderByDescending(cr => cr.ConnectionRequestId)
                .AsNoTracking();
        }

        public async Task<bool> ExistsAcceptedAsync(int investorId, int projectId)
        {
            return await _context.ConnectionRequests
                .AnyAsync(cr => cr.InvestorId == investorId
                    && cr.ProjectId == projectId
                    && cr.Status == ConnectionRequestStatus.Accepted);
        }

        public async Task AddAsync(ConnectionRequest request)
        {
            await _context.ConnectionRequests.AddAsync(request);
        }

        public void Update(ConnectionRequest request)
        {
            _context.ConnectionRequests.Update(request);
        }
    }
}
