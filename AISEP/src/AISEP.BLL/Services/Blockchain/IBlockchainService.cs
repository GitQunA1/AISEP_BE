namespace AISEP.BLL.Services.Blockchain
{
    public interface IBlockchainService
    {
        Task<string> StoreHashAsync(string fileHash, int entityId);
        Task<string> ComputeFileHashAsync(IFormFile file);
        Task<(int EntityId, long Timestamp)> VerifyDocumentAsync(string fileHash);
    }
}
