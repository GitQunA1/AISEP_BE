using AISEP.Common;
using AISEP.DTOs.Requests;
using AISEP.DTOs.Responses;
using AISEP.Models.Entities;
using AISEP.Services.Blockchain;
using AISEP.Services.Storage;
using AutoMapper;

namespace AISEP.Services.Documents
{
    /// <summary>
    /// Service chính cho Document.
    /// - Gọi IStorageService để upload file lên Cloudinary.
    /// - Gọi IBlockchainService để lưu hash lên Sepolia (nếu IsIpProtected).
    /// - Lưu thông tin cuối cùng vào SQL Database (bảng Documents) qua UnitOfWork.
    /// 
    /// Logger tối ưu:
    /// - Chỉ log ở đầu và cuối hàm chính.
    /// - Log chi tiết nằm trong các service con (StorageService, BlockchainService).
    /// - Chỉ log Error khi có Exception.
    /// </summary>
    public class DocumentService : IDocumentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStorageService _storageService;
        private readonly IBlockchainService _blockchainService;
        private readonly ILogger<DocumentService> _logger;
        private readonly IMapper _mapper;

        public DocumentService(
            IUnitOfWork unitOfWork,
            IStorageService storageService,
            IBlockchainService blockchainService,
            ILogger<DocumentService> logger,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _storageService = storageService;
            _blockchainService = blockchainService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<DocumentResponse> UploadDocumentAsync(UploadDocumentRequest dto)
        {
            _logger.LogInformation("UploadDocument started — StartupId: {StartupId}, FileName: {FileName}, IpProtected: {IpProtected}",
                dto.StartupId, dto.File.FileName, dto.IsIpProtected);

            try
            {
                // 1. Upload file lên Cloudinary (log chi tiết ở trong StorageService)
                var fileUrl = await _storageService.UploadFileAsync(dto.File);

                // 2. Nếu cần IP Protection → tính hash + lưu Blockchain
                string? fileHash = null;
                string? txHash = null;
                DateTime? verifiedAt = null;

                if (dto.IsIpProtected)
                {
                    fileHash = await _blockchainService.ComputeFileHashAsync(dto.File);
                    txHash = await _blockchainService.StoreHashAsync(fileHash, dto.StartupId);
                    verifiedAt = DateTime.UtcNow;
                }

                // 3. Lưu vào Database
                var document = new Document
                {
                    ProjectId = dto.StartupId,
                    DocumentType = dto.DocumentType,
                    FileName = dto.File.FileName,
                    FileUrl = fileUrl,
                    FileHash = fileHash,
                    BlockchainTxHash = txHash,
                    IsIpProtected = dto.IsIpProtected,
                    VerifiedAt = verifiedAt
                };

                await _unitOfWork.Documents.AddAsync(document);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<DocumentResponse>(document);

                _logger.LogInformation("UploadDocument completed — DocumentId: {DocumentId}", document.DocumentId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UploadDocument failed — StartupId: {StartupId}, FileName: {FileName}",
                    dto.StartupId, dto.File.FileName);
                throw;
            }
        }

        public async Task<DocumentResponse?> GetByIdAsync(int id)
        {
            var document = await _unitOfWork.Documents.GetByIdAsync(id);
            if (document is null)
                return null;

            return _mapper.Map<DocumentResponse>(document);
        }

        public async Task<IEnumerable<DocumentResponse>> GetByStartupIdAsync(int startupId)
        {
            var documents = await _unitOfWork.Documents.GetByStartupIdAsync(startupId);
            return documents.Select(d => _mapper.Map<DocumentResponse>(d));
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var document = await _unitOfWork.Documents.GetByIdAsync(id);

            if (document == null)
                return false;

            _unitOfWork.Documents.Delete(document);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Document deleted — DocumentId: {DocumentId}", id);
            return true;
        }

    }
}

