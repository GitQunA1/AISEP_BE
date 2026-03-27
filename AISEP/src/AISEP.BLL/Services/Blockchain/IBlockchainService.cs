using AISEP.BLL.DTOs.Responses;

namespace AISEP.BLL.Services.Blockchain
{
    public interface IBlockchainService
    {
        Task<string> StoreHashAsync(string fileHash, int entityId);
        Task<string> ComputeFileHashAsync(IFormFile file);
        Task<string> ComputeFileHashFromUrlAsync(string fileUrl);
        Task<(int EntityId, long Timestamp)> VerifyDocumentAsync(string fileHash);
        Task<ProjectBlockchainVerificationResponse> VerifyProjectDocumentsAsync(int projectId);
    }
}
