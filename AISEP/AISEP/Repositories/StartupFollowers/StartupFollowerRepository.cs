using AISEP.Data;
using AISEP.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.Repositories.StartupFollowers
{
    public class StartupFollowerRepository : IStartupFollowerRepository
    {
        private readonly ApplicationDbContext _context;
        public StartupFollowerRepository(ApplicationDbContext context) { _context = context; }

        public async Task AddAsync(StartupFollower startupFollower)
        {
            await _context.StartupFollowers.AddAsync(startupFollower);
        }

        public async Task RemoveAsync(Guid userId, Guid startupId)
        {
            var follower = await _context.StartupFollowers
                .FirstOrDefaultAsync(sf => sf.UserId == userId && sf.StartupId == startupId);
            if (follower != null)
            {
                _context.StartupFollowers.Remove(follower);
            }
        }

        public async Task<StartupFollower?> GetByIdAsync(Guid userId, Guid startupId)
        {
            return await _context.StartupFollowers
                .Include(sf => sf.User)
                .Include(sf => sf.Startup)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(sf => sf.UserId == userId && sf.StartupId == startupId);
        }

        public async Task<bool> IsFollowingAsync(Guid userId, Guid startupId)
        {
            return await _context.StartupFollowers
                .AnyAsync(sf => sf.UserId == userId && sf.StartupId == startupId);
        }

        public IQueryable<StartupFollower> GetFollowerQuery()
        {
            return _context.StartupFollowers
                .Include(sf => sf.User)
                .Include(sf => sf.Startup)
                    .ThenInclude(s => s.User)
                .AsNoTracking();
        }
    }
}
