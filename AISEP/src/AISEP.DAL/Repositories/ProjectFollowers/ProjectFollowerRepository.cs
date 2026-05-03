using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.ProjectFollowers
{
    public class ProjectFollowerRepository : IProjectFollowerRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectFollowerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ProjectFollower projectFollower)
        {
            await _context.ProjectFollowers.AddAsync(projectFollower);
        }

        public async Task RemoveAsync(int userId, int projectId)
        {
            var follower = await _context.ProjectFollowers
                .FirstOrDefaultAsync(pf => pf.FollowerId == userId && pf.ProjectId == projectId);
            if (follower is not null)
            {
                _context.ProjectFollowers.Remove(follower);
            }
        }

        public async Task<ProjectFollower?> GetByIdAsync(int userId, int projectId)
        {
            return await _context.ProjectFollowers
                .Include(pf => pf.User)
                .Include(pf => pf.Project)
                .FirstOrDefaultAsync(pf => pf.FollowerId == userId && pf.ProjectId == projectId);
        }

        public async Task<bool> IsFollowingAsync(int userId, int projectId)
        {
            return await _context.ProjectFollowers
                .AnyAsync(pf => pf.FollowerId == userId && pf.ProjectId == projectId);
        }

        public IQueryable<ProjectFollower> GetFollowerQuery()
        {
            return _context.ProjectFollowers
                .Include(pf => pf.User)
                .Include(pf => pf.Project)
                .ThenInclude(p => p.IndustryOption)
                .AsNoTracking();
        }
    }
}
