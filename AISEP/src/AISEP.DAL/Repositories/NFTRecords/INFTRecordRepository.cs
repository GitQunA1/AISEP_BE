using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.NFTRecords
{
    public interface INFTRecordRepository
    {
        Task<NFTRecord?> GetByDealIdAsync(int dealId);
        Task AddAsync(NFTRecord nftRecord);
    }
}
