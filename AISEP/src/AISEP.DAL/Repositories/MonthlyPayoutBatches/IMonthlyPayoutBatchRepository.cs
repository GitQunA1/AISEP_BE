using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.MonthlyPayoutBatches
{
    public interface IMonthlyPayoutBatchRepository
    {
        Task<MonthlyPayoutBatch?> GetByPeriodAsync(int year, int month);
        Task<MonthlyPayoutBatch?> GetByIdAsync(int id);
        IQueryable<MonthlyPayoutBatch> GetQuery();
        Task AddAsync(MonthlyPayoutBatch batch);
        void Update(MonthlyPayoutBatch batch);
    }
}
