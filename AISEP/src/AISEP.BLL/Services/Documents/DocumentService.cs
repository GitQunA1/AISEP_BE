using AISEP.DAL.Common;
using AISEP.BLL.Common;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AISEP.BLL.Services.Blockchain;
using AISEP.BLL.Services.Storage;
using AutoMapper;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.BLL.Services.Documents
{
    public class DocumentService : IDocumentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStorageService _storageService;
        private readonly IBlockchainService _blockchainService;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper _mapper;

        public DocumentService(
            IUnitOfWork unitOfWork,
            IStorageService storageService,
            IBlockchainService blockchainService,
            ISieveProcessor sieveProcessor,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _storageService = storageService;
            _blockchainService = blockchainService;
            _sieveProcessor = sieveProcessor;
            _mapper = mapper;
        }

        public async Task<DocumentResponse> UploadDocumentAsync(int projectId, int userId, UploadDocumentRequest request)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            if (project is null)
                throw new KeyNotFoundException("Project not found.");

            var startup = await _unitOfWork.Startups.GetByUserIdAsync(userId);
            if (startup is null || project.StartupId != startup.StartupId)
                throw new UnauthorizedAccessException("You do not have permission to upload documents to this project.");

            var fileUrl = await _storageService.UploadFileAsync(request.File);
            var fileHash = await _blockchainService.ComputeFileHashAsync(request.File);
            var txHash = await _blockchainService.StoreHashAsync(fileHash, projectId);

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

        public async Task<DocumentResponse?> GetByIdAsync(int id, int userId, string role)
        {
            var document = await _unitOfWork.Documents.GetByIdAsync(id);
            if (document is null)
                return null;

            if (role == "Startup")
            {
                var startup = await _unitOfWork.Startups.GetByUserIdAsync(userId);
                if (startup is null || document.Project.StartupId != startup.StartupId)
                    throw new UnauthorizedAccessException("You do not have permission to access this document.");
            }

            return _mapper.Map<DocumentResponse>(document);
        }

        public async Task<PagedResult<DocumentResponse>> GetByProjectIdAsync(int projectId, int userId, string role, SieveModel model)
        {
            if (role == "Startup")
            {
                var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
                if (project is null)
                    throw new KeyNotFoundException("Project not found.");

                var startup = await _unitOfWork.Startups.GetByUserIdAsync(userId);
                if (startup is null || project.StartupId != startup.StartupId)
                    throw new UnauthorizedAccessException("You do not have permission to access documents of this project.");
            }

            var query = _unitOfWork.Documents.GetQueryable()
                .Where(d => d.ProjectId == projectId);

            return await PaginationHelper.PaginateAsync(
                query, model, _sieveProcessor, d => _mapper.Map<DocumentResponse>(d));
        }

        public async Task<bool> DeleteAsync(int id, int userId, string role)
        {
            var document = await _unitOfWork.Documents.GetByIdAsync(id);
            if (document is null)
                return false;

            if (role == "Startup")
            {
                var startup = await _unitOfWork.Startups.GetByUserIdAsync(userId);
                if (startup is null || document.Project.StartupId != startup.StartupId)
                    throw new UnauthorizedAccessException("You do not have permission to delete this document.");
            }

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
