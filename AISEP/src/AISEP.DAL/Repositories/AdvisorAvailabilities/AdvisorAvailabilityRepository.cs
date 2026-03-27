using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.AdvisorAvailabilities
{
    public class AdvisorAvailabilityRepository : IAdvisorAvailabilityRepository
    {
        private readonly ApplicationDbContext _context;

        public AdvisorAvailabilityRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<AdvisorAvailability> GetQuery()
            => _context.AdvisorAvailabilities
                .Include(x => x.Advisor)
                .AsNoTracking()
                .AsQueryable();

        public async Task<AdvisorAvailability?> GetByIdAsync(int id)
            => await _context.AdvisorAvailabilities
                .Include(x => x.Advisor)
                .Include(x => x.BookingSlots)
                .FirstOrDefaultAsync(x => x.AdvisorAvailabilityId == id);

        public async Task<List<AdvisorAvailability>> GetByIdsAsync(IEnumerable<int> ids)
            => await _context.AdvisorAvailabilities
                .Where(x => ids.Contains(x.AdvisorAvailabilityId))
                .OrderBy(x => x.SlotDate)
                .ThenBy(x => x.StartTime)
                .ToListAsync();

        public async Task AddAsync(AdvisorAvailability availability)
            => await _context.AdvisorAvailabilities.AddAsync(availability);

        public void Update(AdvisorAvailability availability)
            => _context.AdvisorAvailabilities.Update(availability);

        public async Task DeleteAsync(int id)
            => await _context.AdvisorAvailabilities
                .Where(x => x.AdvisorAvailabilityId == id)
                .ExecuteDeleteAsync();

        public async Task<bool> ExistsAsync(int advisorId, DateTime slotDate, TimeOnly startTime, TimeOnly endTime, int? excludeId = null)
            => await _context.AdvisorAvailabilities
                .Where(x => x.AdvisorId == advisorId)
                .Where(x => x.SlotDate.Date == slotDate.Date)
                .Where(x => !excludeId.HasValue || x.AdvisorAvailabilityId != excludeId.Value)
                .AnyAsync(x => x.StartTime == startTime && x.EndTime == endTime);
    }
}
