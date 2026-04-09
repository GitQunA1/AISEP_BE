using AISEP.BLL.DTOs.Responses;

namespace AISEP.BLL.Services.Blockchain
{
    public interface IBlockchainService
    {
        Task<string> RegisterDocumentAsync(string fileHash, int startupId);
        Task<string> AddDocumentOwnerAsync(string fileHash, string investorWallet);
        Task<string> ComputeFileHashAsync(IFormFile file);
        Task<string> ComputeFileHashFromUrlAsync(string fileUrl);
        Task<(int StartupId, long Timestamp, IReadOnlyList<string> Owners)> VerifyDocumentAsync(string fileHash);
        Task<ProjectBlockchainVerificationResponse> VerifyProjectDocumentsAsync(int projectId);
    }
}
