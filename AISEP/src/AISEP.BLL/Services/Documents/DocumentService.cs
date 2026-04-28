using AISEP.DAL.Common;
using AISEP.BLL.Helpers;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AISEP.BLL.Services.Blockchain;
using AISEP.BLL.Services.Storage;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Nethereum.ABI.FunctionEncoding;
using Sieve.Models;
using Sieve.Services;
using AISEP.BLL.Services.Users;
using AISEP.BLL.Services.ProjectAdvisorAssignments;

namespace AISEP.BLL.Services.Documents
{
    public class DocumentService : IDocumentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStorageService _storageService;
        private readonly IBlockchainService _blockchainService;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper _mapper;
        private readonly IUserService _currentUserService;
        private readonly IProjectAdvisorAutoAssignService _projectAdvisorAutoAssignService;

        public DocumentService(
            IUnitOfWork unitOfWork,
            IStorageService storageService,
            IBlockchainService blockchainService,
            ISieveProcessor sieveProcessor,
            IMapper mapper,
            IUserService currentUserService,
            IProjectAdvisorAutoAssignService projectAdvisorAutoAssignService
            )
        {
            _unitOfWork = unitOfWork;
            _storageService = storageService;
            _blockchainService = blockchainService;
            _sieveProcessor = sieveProcessor;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _projectAdvisorAutoAssignService = projectAdvisorAutoAssignService;
        }

        public async Task<DocumentResponse> UploadDocumentAsync(int projectId, int userId, UploadDocumentRequest request)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            if (project is null)
                throw new KeyNotFoundException("Project not found.");

            var startup = await _unitOfWork.Startups.GetByUserIdAsync(userId);
            if (startup is null || project.StartupId != startup.StartupId)
                throw new UnauthorizedAccessException("You do not have permission to upload documents to this project.");
            EnsureStartupApproved(startup);

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
            long timestampOnChain;
            try
            {
                (_, timestampOnChain, _) = await _blockchainService.VerifyDocumentAsync(fileHash);
            }
            catch (SmartContractRevertException ex)
            {
                if (ex.Message.Contains("Document hash not found", StringComparison.OrdinalIgnoreCase))
                {
                    timestampOnChain = 0;
                }
                else
                {
                    throw new InvalidOperationException($"Không thể kiểm tra hash trên blockchain: {ex.Message}");
                }
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Không thể kiểm tra hash trên blockchain: {ex.Message}");
            }

            if (timestampOnChain > 0)
            {
                throw new InvalidOperationException(
                    "File đã tồn tại trên hệ thống (đã được đăng ký trên blockchain). Không thể upload lại cùng nội dung.");
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

            await EnsureCanViewProjectDocumentsAsync(document.ProjectId, userId, role);

            return _mapper.Map<DocumentResponse>(document);
        }

        public async Task<PagedResult<DocumentResponse>> GetByProjectIdAsync(int projectId, int userId, string role, SieveModel model)
        {
            await EnsureCanViewProjectDocumentsAsync(projectId, userId, role);

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
                EnsureStartupApproved(startup);
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

            var (startupId, timestamp, _) = await _blockchainService.VerifyDocumentAsync(document.FileHash);

            if (timestamp <= 0)
            {
                return new BlockchainVerificationResponse
                {
                    IsAuthentic = false,
                    TxHash = document.BlockchainTxHash,
                    TimestampOnBlockchain = string.Empty,
                    Message = "Document hash was not found on the blockchain."
                };
            }

            var isAuthentic = startupId == document.ProjectId;

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

        public async Task<DocumentResponse> ApproveProjectAsync(int projectId)
        {
            var userId = _currentUserService.GetUserId();
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            if (project is null)
                throw new KeyNotFoundException("Project not found.");

            if (project.Status != ProjectStatus.Pending)
                throw new InvalidOperationException("Chỉ duyệt dự án đang chờ duyệt (Pending).");

            var document = _unitOfWork.Documents.GetQueryable()
                .FirstOrDefault(d => d.ProjectId == projectId);
            if (document is null)
                throw new InvalidOperationException("Không tìm thấy tài liệu gắn với dự án này. Vui lòng upload tài liệu trước khi duyệt.");

            if (string.IsNullOrEmpty(document.FileHash))
                throw new InvalidOperationException("Tài liệu chưa có thông tin hash. Vui lòng upload lại tài liệu.");

            string txHash;
            try
            {
                txHash = await _blockchainService.RegisterDocumentAsync(document.FileHash, projectId);
            }
            catch (SmartContractRevertException ex)
            {
                if (ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "Document hash already exists on blockchain. This project may have been approved before or the same file was already registered.");
                }

                throw new InvalidOperationException($"Blockchain transaction failed: {ex.Message}");
            }

            document.BlockchainTxHash = txHash;
            document.IsIpProtected = true;
            document.VerifiedAt = DateTime.UtcNow;
            _unitOfWork.Documents.Update(document);

            project.Status = ProjectStatus.Approved;
            project.ApprovedAt = DateTime.UtcNow;
            project.ApprovedById = userId;
            _unitOfWork.Projects.Update(project);

            await _projectAdvisorAutoAssignService.TryAssignAdvisorAsync(project);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<DocumentResponse>(document);
        }

        private async Task EnsureCanViewProjectDocumentsAsync(int projectId, int userId, string role)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId)
                ?? throw new KeyNotFoundException("Project not found.");

            if (string.Equals(role, "Staff", StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (string.Equals(role, "Startup", StringComparison.OrdinalIgnoreCase))
            {
                var startup = await _unitOfWork.Startups.GetByUserIdAsync(userId);
                var isOwnProject = startup is not null && project.StartupId == startup.StartupId;
                if (isOwnProject)
                {
                    return;
                }

                var isUnlocked = await _unitOfWork.UnlockedProjects.ExistsAsync(userId, projectId);
                if (!isUnlocked)
                {
                    throw new UnauthorizedAccessException("Bạn cần mở khóa dự án trước khi xem tài liệu của startup khác.");
                }

                return;
            }

            if (string.Equals(role, "Advisor", StringComparison.OrdinalIgnoreCase))
            {
                var advisor = await _unitOfWork.Advisors.GetByUserIdAsync(userId);
                if (advisor is null)
                {
                    throw new UnauthorizedAccessException("Advisor profile not found.");
                }

                var isAssigned = await _unitOfWork.ProjectAdvisorAssignments
                    .GetByAdvisorIdQuery(advisor.AdvisorId)
                    .AnyAsync(x => x.ProjectId == projectId);

                if (!isAssigned)
                {
                    throw new UnauthorizedAccessException("Bạn chỉ được xem tài liệu của dự án đã được phân công cố vấn.");
                }

                return;
            }

            if (string.Equals(role, "Investor", StringComparison.OrdinalIgnoreCase))
            {
                var isUnlocked = await _unitOfWork.UnlockedProjects.ExistsAsync(userId, projectId);
                if (!isUnlocked)
                {
                    throw new UnauthorizedAccessException("Bạn cần mở khóa dự án trước khi xem tài liệu.");
                }

                return;
            }

            throw new UnauthorizedAccessException("You do not have permission to access documents of this project.");
        }

        private static void EnsureStartupApproved(Startup startup)
        {
            if (startup.ApprovalStatus != ApprovalStatus.Approved)
                throw new InvalidOperationException("Your startup profile must be approved before using this feature.");
        }
    }
}
