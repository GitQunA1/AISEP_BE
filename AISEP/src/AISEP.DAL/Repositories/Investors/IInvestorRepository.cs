using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.Investors
{
    public interface IInvestorRepository
    {
        IQueryable<Investor> GetAllQuery();
        IQueryable<Investor> GetStartupMatchingInvestorsQuery(IEnumerable<int> industryOptionIds, IEnumerable<int> stageOptionIds);
        Task<Investor?> GetByIdAsync(int investorId);
        Task<Investor?> GetByUserIdAsync(int userId);
        Task AddAsync(Investor investor);
        void Update(Investor investor);
        Task<int> SaveChangesAsync();
    }
}
