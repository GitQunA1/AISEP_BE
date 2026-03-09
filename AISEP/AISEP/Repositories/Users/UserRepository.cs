using AISEP.Data;
using AISEP.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.Repositories.Users
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<User?> GetByProjectId(int id)
        {
            return await _context.Users
                .Include(u => u.Startup)
                    .ThenInclude(s => s.Projects)
                .FirstOrDefaultAsync(u => u.Startup != null &&
                                          u.Startup.Projects.Any(p => p.ProjectId == id));
        }
    }
}
