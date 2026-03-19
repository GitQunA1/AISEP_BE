using AISEP.DAL.Common;
using AISEP.BLL.Helpers;
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

            if (project.Status != ProjectStatus.Draft)
                throw new InvalidOperationException("Chỉ được phép upload tài liệu khi dự án ở trạng thái DRAFT.");

            // Tính hash từ file đang trong memory (trước khi upload)
            var fileHash = await _blockchainService.ComputeFileHashAsync(request.File);

            // Kiểm tra file đã tồn tại trong hệ thống chưa (chặn duplicate trước khi đẩy blockchain)
            var isDuplicate = _unitOfWork.Documents.GetQueryable()
                .Any(d => d.FileHash == fileHash);
            if (isDuplicate)
                throw new InvalidOperationException("Tài liệu này đã được upload trước đó. Vui lòng kiểm tra lại.");

            // Kiểm tra hash đã tồn tại on-chain chưa để tránh bypass khi dữ liệu DB bị xóa.
            var (_, timestampOnChain) = await _blockchainService.VerifyDocumentAsync(fileHash);
            if (timestampOnChain > 0)
            {
                throw new InvalidOperationException(
                    "Tài liệu này đã từng được đăng ký trên blockchain trước đó. Không thể upload lại cùng nội dung.");
            }

            var fileUrl = await _storageService.UploadFileAsync(request.File);

            var document = new Document
            {
                ProjectId        = projectId,
                DocumentType     = request.DocumentType,
                FileName         = request.File.FileName,
                FileUrl          = fileUrl,
                FileHash         = fileHash,       // lưu hash ngay để dùng lúc approve
                BlockchainTxHash = null,
                IsIpProtected    = false
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

            var lockedStatuses = new[] { ProjectStatus.Approved};
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

        public async Task<DocumentResponse> ApproveProjectAsync(int projectId, int staffUserId)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            if (project is null)
                throw new KeyNotFoundException("Project not found.");

            if (project.Status != ProjectStatus.Pending)
                throw new InvalidOperationException("Chỉ duyệt dự án đang chờ duyệt (Pending).");

            // Find document attached to this project
            var document = _unitOfWork.Documents.GetQueryable()
                .FirstOrDefault(d => d.ProjectId == projectId);
            if (document is null)
                throw new InvalidOperationException("Không tìm thấy tài liệu gắn với dự án này. Vui lòng upload tài liệu trước khi duyệt.");

            if (string.IsNullOrEmpty(document.FileHash))
                throw new InvalidOperationException("Tài liệu chưa có thông tin hash. Vui lòng upload lại tài liệu.");

            // Store hash trên blockchain (Sepolia) - dùng hash đã tính lúc upload
            var txHash = await _blockchainService.StoreHashAsync(document.FileHash, projectId);

            // Update document với thông tin blockchain
            document.BlockchainTxHash = txHash;
            document.IsIpProtected    = true;
            document.VerifiedAt       = DateTime.UtcNow;
            _unitOfWork.Documents.Update(document);

            // Approve project
            project.Status       = ProjectStatus.Approved;
            project.ApprovedAt   = DateTime.UtcNow;
            project.ApprovedById = staffUserId;
            _unitOfWork.Projects.Update(project);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<DocumentResponse>(document);
        }
    }
}
