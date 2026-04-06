using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.Wallets
{
    public class WalletRepository : IWalletRepository
    {
        private readonly ApplicationDbContext _context;

        public WalletRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Wallet?> GetByAdvisorIdAsync(int advisorId)
            => await _context.Wallets.FirstOrDefaultAsync(x => x.AdvisorId == advisorId);

        public async Task AddAsync(Wallet wallet)
            => await _context.Wallets.AddAsync(wallet);

        public void Update(Wallet wallet)
            => _context.Wallets.Update(wallet);
    }
}
