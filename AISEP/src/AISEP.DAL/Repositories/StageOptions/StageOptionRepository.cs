using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.StageOptions
{
    public class StageOptionRepository : IStageOptionRepository
    {
        private readonly ApplicationDbContext _context;

        public StageOptionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<StageOption> GetAllQuery()
            => _context.StageOptions.OrderBy(x => x.Value).AsQueryable();

        public IQueryable<StageOption> GetActiveQuery()
            => _context.StageOptions.Where(x => x.IsActive).OrderBy(x => x.Value).AsQueryable();

        public Task<StageOption?> GetByIdAsync(int id)
            => _context.StageOptions.FirstOrDefaultAsync(x => x.Id == id);

        public Task<List<StageOption>> GetByIdsAsync(IEnumerable<int> ids)
        {
            var distinctIds = ids.Distinct().ToList();
            return _context.StageOptions
                .Where(x => distinctIds.Contains(x.Id))
                .ToListAsync();
        }

        public Task AddAsync(StageOption option)
            => _context.StageOptions.AddAsync(option).AsTask();

        public void Update(StageOption option)
            => _context.StageOptions.Update(option);
    }
}
