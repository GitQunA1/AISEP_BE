using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.Advisors
{
    public interface IAdvisorsRepository
    {
        IQueryable<Advisor> GetAllQuery();
        Task<Advisor?> GetByIdAsync(int id);
        Task<Advisor?> GetByUserIdAsync(int userId);
        Task AddAsync(Advisor advisor);
        void Update(Advisor advisor);
        Task DeleteAsync(int id);
    }
}
