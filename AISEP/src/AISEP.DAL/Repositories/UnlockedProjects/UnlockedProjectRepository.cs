using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.UnlockedProjects
{
    public class UnlockedProjectRepository : IUnlockedProjectRepository
    {
        private readonly ApplicationDbContext _context;

        public UnlockedProjectRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(int userId, int projectId)
            => await _context.UnlockedProjects.AnyAsync(x => x.UserId == userId && x.ProjectId == projectId);

        public async Task AddAsync(UnlockedProject unlockedProject)
            => await _context.UnlockedProjects.AddAsync(unlockedProject);
    }
}
