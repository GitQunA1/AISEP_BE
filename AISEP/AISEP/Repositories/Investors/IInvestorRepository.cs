using AISEP.Models.Entities;

namespace AISEP.Repositories.Investors
{
    public interface IInvestorRepository
    {
        IQueryable<Investor> GetAllQuery();
        Task<Investor?> GetByIdAsync(int investorId);
        Task<Investor?> GetByUserIdAsync(int userId);
        Task AddAsync(Investor investor);
        void Update(Investor investor);
        Task<int> SaveChangesAsync();
    }
}
