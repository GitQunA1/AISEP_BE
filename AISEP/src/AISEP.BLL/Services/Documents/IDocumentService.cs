using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;

namespace AISEP.BLL.Services.Documents
{
    public interface IDocumentService
    {
        Task<DocumentResponse> UploadDocumentAsync(int projectId, UploadDocumentRequest request);
        Task<DocumentResponse?> GetByIdAsync(int id);
        Task<IEnumerable<DocumentResponse>> GetByProjectIdAsync(int projectId);
        Task<bool> DeleteAsync(int id);
        Task<BlockchainVerificationResponse> VerifyDocumentAsync(int documentId);
    }
}
