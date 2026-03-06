using AISEP.DTOs.Requests;
using AISEP.DTOs.Responses;

namespace AISEP.Services.Documents
{
    /// <summary>
    /// Service chính cho Document.
    /// Orchestrate: IStorageService + IBlockchainService + Repository (qua UnitOfWork).
    /// Chịu trách nhiệm toàn bộ business logic và lưu DB.
    /// </summary>
    public interface IDocumentService
    {
        /// <summary>
        /// Upload document: Cloudinary + (tuỳ chọn) Blockchain + lưu DB.
        /// </summary>
        Task<DocumentResponse> UploadDocumentAsync(UploadDocumentRequest dto);

        /// <summary>
        /// Lấy document theo Id.
        /// </summary>
        Task<DocumentResponse?> GetByIdAsync(int id);

        /// <summary>
        /// Lấy danh sách document theo StartupId.
        /// </summary>
        Task<IEnumerable<DocumentResponse>> GetByStartupIdAsync(int startupId);

        /// <summary>
        /// Xoá document theo Id.
        /// </summary>
        Task<bool> DeleteAsync(int id);
    }
}
