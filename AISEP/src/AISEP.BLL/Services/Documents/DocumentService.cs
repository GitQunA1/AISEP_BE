using AISEP.DAL.Common;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AISEP.BLL.Services.Blockchain;
using AISEP.BLL.Services.Storage;
using AutoMapper;

namespace AISEP.BLL.Services.Documents
{
    /// <summary>
    /// - Gọi IStorageService để upload file lên Cloudinary.
    /// - Gọi IBlockchainService để lưu hash lên Sepolia (nếu IsIpProtected).
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

        public async Task<DocumentResponse> UploadDocumentAsync(int projectId, UploadDocumentRequest request)
        {
            // 1. Upload file lên Cloudinary
            var fileUrl = await _storageService.UploadFileAsync(request.File);

            // 2. Tính hash + lưu Blockchain (mọi document đều được bảo vệ IP)
            var fileHash = await _blockchainService.ComputeFileHashAsync(request.File);
            var txHash = await _blockchainService.StoreHashAsync(fileHash, projectId);

            // 3. Lưu vào Database
            var document = new Document
            {
                ProjectId        = projectId,
                DocumentType     = request.DocumentType,
                FileName         = request.File.FileName,
                FileUrl          = fileUrl,
                FileHash         = fileHash,
                BlockchainTxHash = txHash,
                IsIpProtected    = true,
                VerifiedAt       = DateTime.UtcNow
            };

            await _unitOfWork.Documents.AddAsync(document);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<DocumentResponse>(document);
        }

        public async Task<DocumentResponse?> GetByIdAsync(int id)
        {
            var document = await _unitOfWork.Documents.GetByIdAsync(id);
            if (document is null)
                return null;

            return _mapper.Map<DocumentResponse>(document);
        }

        public async Task<IEnumerable<DocumentResponse>> GetByProjectIdAsync(int projectId)
        {
            var documents = await _unitOfWork.Documents.GetByProjectIdAsync(projectId);
            return documents.Select(d => _mapper.Map<DocumentResponse>(d));
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var document = await _unitOfWork.Documents.GetByIdAsync(id);

            if (document == null)
                return false;

            // Block deletion if the project is already submitted/approved/published
            var lockedStatuses = new[] { ProjectStatus.Submitted, ProjectStatus.Approved, ProjectStatus.Published };
            if (lockedStatuses.Contains(document.Project.Status))
                throw new InvalidOperationException(
                    $"Cannot delete document: project is in '{document.Project.Status}' status and is locked.");

            _unitOfWork.Documents.Delete(document);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<BlockchainVerificationResponse> VerifyDocumentAsync(int documentId)
        {
            var document = await _unitOfWork.Documents.GetByIdAsync(documentId);
            if (document is null)
                throw new KeyNotFoundException($"Document with Id {documentId} not found.");

            if (string.IsNullOrEmpty(document.FileHash) || string.IsNullOrEmpty(document.BlockchainTxHash))
                throw new InvalidOperationException("This document was not registered on the blockchain.");

            // Gọi Smart Contract (view function — miễn phí gas)
            var (entityId, timestamp) = await _blockchainService.VerifyDocumentAsync(document.FileHash);

            var isAuthentic = timestamp > 0 && entityId == document.ProjectId;

            return new BlockchainVerificationResponse
            {
                IsAuthentic = isAuthentic,
                TxHash = document.BlockchainTxHash,
                TimestampOnBlockchain = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                Message = isAuthentic
                    ? "Document is authentic and protected on the Blockchain."
                    : "Document does not match blockchain data. It may have been tampered with."
            };
        }

    }
}

