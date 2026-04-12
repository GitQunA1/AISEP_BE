using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.MonthlyPayoutBatches
{
    public class MonthlyPayoutBatchRepository : IMonthlyPayoutBatchRepository
    {
        private readonly ApplicationDbContext _context;

        public MonthlyPayoutBatchRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MonthlyPayoutBatch?> GetByPeriodAsync(int year, int month)
            => await _context.MonthlyPayoutBatches
                .Include(x => x.MonthlyPayouts)
                .FirstOrDefaultAsync(x => x.Year == year && x.Month == month);

        public async Task<MonthlyPayoutBatch?> GetByIdAsync(int id)
            => await _context.MonthlyPayoutBatches
                .Include(x => x.MonthlyPayouts)
                .FirstOrDefaultAsync(x => x.MonthlyPayoutBatchId == id);

        public IQueryable<MonthlyPayoutBatch> GetQuery()
            => _context.MonthlyPayoutBatches
                .Include(x => x.MonthlyPayouts)
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .ThenByDescending(x => x.MonthlyPayoutBatchId)
                .AsNoTracking();

        public async Task AddAsync(MonthlyPayoutBatch batch)
            => await _context.MonthlyPayoutBatches.AddAsync(batch);

        public void Update(MonthlyPayoutBatch batch)
            => _context.MonthlyPayoutBatches.Update(batch);
    }
}
