using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.MonthlyPayouts
{
    public interface IMonthlyPayoutRepository
    {
        Task<MonthlyPayout?> GetByIdAsync(int monthlyPayoutId);
        Task<MonthlyPayout?> GetByAdvisorAndPeriodAsync(int advisorId, int year, int month);
        Task<bool> ExistsByPeriodAsync(int year, int month);
        IQueryable<MonthlyPayout> GetQuery();
        Task AddAsync(MonthlyPayout monthlyPayout);
        void Update(MonthlyPayout monthlyPayout);
    }
}
