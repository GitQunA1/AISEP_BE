using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.AdvisorBankAccounts
{
    public interface IAdvisorBankAccountRepository
    {
        IQueryable<AdvisorBankAccount> GetAllQuery();
        Task<AdvisorBankAccount?> GetByIdAsync(int id);
        Task<AdvisorBankAccount?> GetActiveByAdvisorIdAsync(int advisorId);
        Task AddAsync(AdvisorBankAccount account);
        void Update(AdvisorBankAccount account);
    }
}
