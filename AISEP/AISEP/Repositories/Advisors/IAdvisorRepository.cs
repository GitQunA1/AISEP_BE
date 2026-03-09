using AISEP.Models.Entities;

namespace AISEP.Repositories.Advisors
{
    public interface IAdvisorsRepository
    {
        Task<Advisor?> GetByIdAsync(Guid id);
        Task DeleteAsync(Guid id);
        IQueryable<Booking> GetBookingQuery();
    }
}
