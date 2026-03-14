using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.Users
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

       

        public IQueryable<User> GetAllQuery()
        {
            return _context.Users.AsQueryable();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByProjectId(int id)
        {
            return await _context.Users
                .Include(u => u.Startup)
                    .ThenInclude(s => s!.Projects)
                .FirstOrDefaultAsync(u => u.Startup != null &&
                                          u.Startup.Projects.Any(p => p.ProjectId == id));
        }
    }
}
