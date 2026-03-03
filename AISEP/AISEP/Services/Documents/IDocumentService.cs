using AISEP.Models.DTOs;

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
        Task<DocumentResponseDto> UploadDocumentAsync(UploadDocumentDto dto);

        /// <summary>
        /// Lấy document theo Id.
        /// </summary>
        Task<DocumentResponseDto?> GetByIdAsync(int id);

        /// <summary>
        /// Lấy danh sách document theo StartupId.
        /// </summary>
        Task<IEnumerable<DocumentResponseDto>> GetByStartupIdAsync(int startupId);

        /// <summary>
        /// Xoá document theo Id.
        /// </summary>
        Task<bool> DeleteAsync(int id);
    }
}
