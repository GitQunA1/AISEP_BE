using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.StartupFollowers
{
    public class StartupFollowerRepository : IStartupFollowerRepository
    {
        private readonly ApplicationDbContext _context;
        public StartupFollowerRepository(ApplicationDbContext context) { _context = context; }

        public async Task AddAsync(StartupFollower startupFollower)
        {
            await _context.StartupFollowers.AddAsync(startupFollower);
        }

        public async Task RemoveAsync(int userId, int startupId)
        {
            var follower = await _context.StartupFollowers
                .FirstOrDefaultAsync(sf => sf.FollowerId == userId && sf.FollowedId == startupId);
            if (follower != null)
            {
                _context.StartupFollowers.Remove(follower);
            }
        }

        public async Task<bool> IsFollowingAsync(int userId, int startupId)
        {
            return await _context.StartupFollowers
                .AnyAsync(sf => sf.FollowerId == userId && sf.FollowedId == startupId);
        }

        public IQueryable<StartupFollower> GetFollowerQuery()
        {
            return _context.StartupFollowers
                .Include(sf => sf.User)
                .Include(sf => sf.Startup)
                    .ThenInclude(s => s.User)
                .AsNoTracking();
        }

        public async Task<StartupFollower?> GetByIdAsync(int userId, int startupId)
        {
            return await _context.StartupFollowers
                .Include(sf => sf.User)
                .Include(sf => sf.Startup)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(sf => sf.FollowerId == userId && sf.FollowedId == startupId);
        }
    }
}
