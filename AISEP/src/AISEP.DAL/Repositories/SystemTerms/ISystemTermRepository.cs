using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.SystemTerms
{
    public interface ISystemTermRepository
    {
        Task<SystemTerm?> GetActiveAsync();
        IQueryable<SystemTerm> GetQuery();
        Task AddAsync(SystemTerm systemTerm);
        void Update(SystemTerm systemTerm);
    }
}
