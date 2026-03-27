using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.AdvisorAvailabilities
{
    public interface IAdvisorAvailabilityRepository
    {
        IQueryable<AdvisorAvailability> GetQuery();
        Task<AdvisorAvailability?> GetByIdAsync(int id);
        Task<List<AdvisorAvailability>> GetByIdsAsync(IEnumerable<int> ids);
        Task AddAsync(AdvisorAvailability availability);
        void Update(AdvisorAvailability availability);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int advisorId, DateTime slotDate, TimeOnly startTime, TimeOnly endTime, int? excludeId = null);
    }
}
