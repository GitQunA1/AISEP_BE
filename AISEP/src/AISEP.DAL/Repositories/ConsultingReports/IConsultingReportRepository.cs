using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.ConsultingReports
{
    public interface IConsultingReportRepository
    {
        Task<ConsultingReport?> GetByIdAsync(int id);
        Task<ConsultingReport?> GetByBookingIdAsync(int bookingId);
        Task AddAsync(ConsultingReport report);
        void Update(ConsultingReport report);
        IQueryable<ConsultingReport> GetQuery();
    }
}
