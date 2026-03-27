using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.NFTRecords
{
    public class NFTRecordRepository : INFTRecordRepository
    {
        private readonly ApplicationDbContext _context;

        public NFTRecordRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<NFTRecord?> GetByDealIdAsync(int dealId)
        {
            return await _context.NFTRecords
                .Include(n => n.Deal)
                .FirstOrDefaultAsync(n => n.DealId == dealId);
        }

        public async Task AddAsync(NFTRecord nftRecord)
        {
            await _context.NFTRecords.AddAsync(nftRecord);
        }
    }
}
