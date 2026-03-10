using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.Advisors
{
    public interface IAdvisorsRepository
    {
        Task<Advisor?> GetByIdAsync(Guid id);
        Task DeleteAsync(Guid id);
        IQueryable<Booking> GetBookingQuery();
    }
}
