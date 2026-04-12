using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.AdvisorBankAccounts
{
    public class AdvisorBankAccountRepository : IAdvisorBankAccountRepository
    {
        private readonly ApplicationDbContext _context;

        public AdvisorBankAccountRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<AdvisorBankAccount> GetAllQuery()
            => _context.AdvisorBankAccounts
                .Include(x => x.Advisor)
                .ThenInclude(x => x.User)
                .OrderByDescending(x => x.UpdatedAt)
                .ThenByDescending(x => x.AdvisorBankAccountId)
                .AsNoTracking();

        public async Task<AdvisorBankAccount?> GetByIdAsync(int id)
            => await _context.AdvisorBankAccounts
                .Include(x => x.Advisor)
                .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(x => x.AdvisorBankAccountId == id);

        public async Task<AdvisorBankAccount?> GetActiveByAdvisorIdAsync(int advisorId)
            => await _context.AdvisorBankAccounts
                .Include(x => x.Advisor)
                .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(x => x.AdvisorId == advisorId && x.IsActive);

        public async Task AddAsync(AdvisorBankAccount account)
            => await _context.AdvisorBankAccounts.AddAsync(account);

        public void Update(AdvisorBankAccount account)
            => _context.AdvisorBankAccounts.Update(account);
    }
}
