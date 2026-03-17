using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using Sieve.Models;

namespace AISEP.BLL.Services.Documents
{
    public interface IDocumentService
    {
        Task<DocumentResponse> UploadDocumentAsync(int projectId, int userId, UploadDocumentRequest request);
        Task<DocumentResponse?> GetByIdAsync(int id, int userId, string role);
        Task<PagedResult<DocumentResponse>> GetByProjectIdAsync(int projectId, int userId, string role, SieveModel model);
        Task<bool> DeleteAsync(int id, int userId, string role);
        Task<BlockchainVerificationResponse> VerifyDocumentAsync(int documentId);
        Task<DocumentResponse> ApproveProjectAsync(int projectId, int staffUserId);
    }
}
