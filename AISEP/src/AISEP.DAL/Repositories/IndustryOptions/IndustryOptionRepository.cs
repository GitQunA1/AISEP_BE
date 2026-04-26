using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.IndustryOptions
{
    public class IndustryOptionRepository : IIndustryOptionRepository
    {
        private readonly ApplicationDbContext _context;

        public IndustryOptionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<IndustryOption> GetAllQuery()
            => _context.IndustryOptions.OrderBy(x => x.Value).AsQueryable();

        public IQueryable<IndustryOption> GetActiveQuery()
            => _context.IndustryOptions.Where(x => x.IsActive).OrderBy(x => x.Value).AsQueryable();

        public Task<IndustryOption?> GetByIdAsync(int id)
            => _context.IndustryOptions.FirstOrDefaultAsync(x => x.Id == id);

        public Task<List<IndustryOption>> GetByIdsAsync(IEnumerable<int> ids)
        {
            var distinctIds = ids.Distinct().ToList();
            return _context.IndustryOptions
                .Where(x => distinctIds.Contains(x.Id))
                .ToListAsync();
        }

        public Task AddAsync(IndustryOption option)
            => _context.IndustryOptions.AddAsync(option).AsTask();

        public void Update(IndustryOption option)
            => _context.IndustryOptions.Update(option);
    }
}
