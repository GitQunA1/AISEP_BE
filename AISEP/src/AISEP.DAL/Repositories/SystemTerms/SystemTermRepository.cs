using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.SystemTerms
{
    public class SystemTermRepository : ISystemTermRepository
    {
        private readonly ApplicationDbContext _context;

        public SystemTermRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SystemTerm?> GetActiveAsync()
            => await _context.SystemTerms
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

        public IQueryable<SystemTerm> GetQuery()
            => _context.SystemTerms
                .OrderByDescending(x => x.CreatedAt)
                .AsNoTracking();

        public async Task AddAsync(SystemTerm systemTerm)
            => await _context.SystemTerms.AddAsync(systemTerm);

        public void Update(SystemTerm systemTerm)
            => _context.SystemTerms.Update(systemTerm);
    }
}
