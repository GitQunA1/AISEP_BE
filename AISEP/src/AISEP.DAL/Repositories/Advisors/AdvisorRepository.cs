using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.Advisors
{
    public class AdvisorRepository : IAdvisorsRepository
    {
        private readonly ApplicationDbContext _context;

        public AdvisorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<Advisor> GetAllQuery()
            => _context.Advisors.Include(a => a.User).OrderBy(a => a.AdvisorId).AsQueryable();

        public async Task<Advisor?> GetByIdAsync(int id)
            => await _context.Advisors
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AdvisorId == id);

        public async Task<Advisor?> GetByUserIdAsync(int userId)
            => await _context.Advisors
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.UserId == userId);

        public async Task AddAsync(Advisor advisor)
            => await _context.Advisors.AddAsync(advisor);

        public void Update(Advisor advisor)
            => _context.Advisors.Update(advisor);

        public async Task DeleteAsync(int id)
            => await _context.Advisors
                .Where(a => a.AdvisorId == id)
                .ExecuteDeleteAsync();
    }
}

