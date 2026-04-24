using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Exceptions;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.BackgroundServices;
using AISEP.BLL.Services.Blockchain;
using AISEP.BLL.Services.Notifications;
using AISEP.BLL.Services.Storage;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Sieve.Models;
using Sieve.Services;
using System.Globalization;
using System.Text;

namespace AISEP.BLL.Services.Deals
{
    public class DealService : IDealService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly IBlockchainService _blockchainService;
        private readonly IBlockchainOwnershipAssignmentQueue _blockchainOwnershipAssignmentQueue;
        private readonly IStorageService _storageService;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<DealService> _logger;

        public DealService(
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            IMapper mapper,
            IBlockchainService blockchainService,
            IBlockchainOwnershipAssignmentQueue blockchainOwnershipAssignmentQueue,
            IStorageService storageService,
            ISieveProcessor sieveProcessor,
            IWebHostEnvironment environment,
            ILogger<DealService> logger)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _mapper = mapper;
            _blockchainService = blockchainService;
            _blockchainOwnershipAssignmentQueue = blockchainOwnershipAssignmentQueue;
            _storageService = storageService;
            _sieveProcessor = sieveProcessor;
            _environment = environment;
            _logger = logger;
        }

        public async Task<DealDto> CreateDealAsync(int investorId, CreateDealDto dto)
        {
            var investor = await _unitOfWork.Investors.GetByIdAsync(investorId)
                ?? throw new KeyNotFoundException("Investor not found.");

            var project = await _unitOfWork.Projects.GetByIdAsync(dto.ProjectId)
                ?? throw new KeyNotFoundException("Project not found.");

            var hasBlockingDeal = await _unitOfWork.Deals.HasBlockingDealAsync(investorId, dto.ProjectId);
            if (hasBlockingDeal)
            {
                throw new InvalidOperationException("You already have an active deal for this project. You can only create a new one after the previous deal is rejected.");
            }

            var deal = _mapper.Map<Deal>(dto);
            deal.InvestorId = investorId;
            deal.InvestorConfirmed = true;
            deal.StartupConfirmed = false;
            deal.Status = DealStatus.Pending;
            deal.DealDate = DateTime.UtcNow;
            deal.IsCompleted = false;

            await _unitOfWork.Deals.AddAsync(deal);
            await _unitOfWork.SaveChangesAsync();

            await _notificationService.SendNotificationAsync(
                project.Startup.UserId,
                "Đề xuất thỏa thuận đầu tư mới",
                $"Có đề xuất thỏa thuận đầu tư mới cho dự án '{project.ProjectName}'.",
                NotificationType.Deal,
                deal.DealId,
                "Deal");

            var created = await _unitOfWork.Deals.GetByIdWithDetailsAsync(deal.DealId)
                ?? throw new KeyNotFoundException("Created deal not found.");

            return _mapper.Map<DealDto>(created);
        }

        public async Task<PagedResult<DealDto>> GetDealsAsync(SieveModel sieveModel)
        {
            sieveModel ??= new SieveModel();

            var query = _unitOfWork.Deals.GetQuery();

            return await PaginationHelper.PaginateAsync(
                query,
                sieveModel,
                _sieveProcessor,
                d => _mapper.Map<DealDto>(d));
        }

        public async Task<PagedResult<DealDto>> GetInvestorDealsAsync(int investorId, SieveModel sieveModel)
        {
            sieveModel ??= new SieveModel();

            var query = _unitOfWork.Deals.GetQuery()
                .Where(d => d.InvestorId == investorId);

            return await PaginationHelper.PaginateAsync(
                query,
                sieveModel,
                _sieveProcessor,
                d => _mapper.Map<DealDto>(d));
        }

        public async Task<PagedResult<DealDto>> GetStartupDealsAsync(int startupId, SieveModel sieveModel)
        {
            sieveModel ??= new SieveModel();

            var query = _unitOfWork.Deals.GetQuery()
                .Where(d => d.Project.StartupId == startupId);

            return await PaginationHelper.PaginateAsync(
                query,
                sieveModel,
                _sieveProcessor,
                d => _mapper.Map<DealDto>(d));
        }

        public async Task<DealDto> RespondDealAsync(int startupId, int dealId, bool isAccepted, string? reason = null)
        {
            var deal = await _unitOfWork.Deals.GetByIdWithDetailsAsync(dealId)
                ?? throw new KeyNotFoundException("Deal not found.");

            if (deal.Project.StartupId != startupId)
            {
                throw new ForbiddenAccessException("You do not have permission to respond to this deal.");
            }

            if (deal.Status != DealStatus.Pending)
            {
                throw new InvalidOperationException("Only pending deals can be responded.");
            }

            var trimmedReason = reason?.Trim();

            string notificationTitle;
            string notificationMessage;

            if (isAccepted)
            {
                deal.StartupConfirmed = true;
                deal.Status = DealStatus.Confirmed;

                notificationTitle = "Thỏa thuận đầu tư đã được xác nhận";
                notificationMessage = "Thỏa thuận đầu tư của bạn đã được startup xác nhận.";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(trimmedReason))
                {
                    throw new InvalidOperationException("Reason is required when deal is rejected.");
                }

                deal.StartupConfirmed = false;
                deal.Status = DealStatus.Rejected;

                notificationTitle = "Thỏa thuận đầu tư bị từ chối";
                notificationMessage = $"Thỏa thuận đầu tư của bạn đã bị startup từ chối. Lý do: {trimmedReason}";
            }

            _unitOfWork.Deals.Update(deal);
            await _unitOfWork.SaveChangesAsync();

            await _notificationService.SendNotificationAsync(
                deal.Investor.UserId,
                notificationTitle,
                notificationMessage,
                NotificationType.Deal,
                deal.DealId,
                "Deal");

            return _mapper.Map<DealDto>(deal);
        }

        public async Task<string> GetContractPreviewForInvestorAsync(int dealId, int investorId)
        {
            var deal = await _unitOfWork.Deals.GetByIdWithDetailsAsync(dealId)
                ?? throw new KeyNotFoundException("Deal not found.");

            EnsureInvestorOwnsDeal(deal, investorId);
            EnsureDealAllowsContractPreview(deal);

            var templateHtml = await ReadContractTemplateAsync();
            return BuildContractHtml(templateHtml, deal);
        }

        public async Task<string> GetContractPreviewAsync(int dealId)
        {
            var deal = await _unitOfWork.Deals.GetByIdWithDetailsAsync(dealId)
                ?? throw new KeyNotFoundException("Deal not found.");

            EnsureDealAllowsContractPreview(deal);

            var templateHtml = await ReadContractTemplateAsync();
            return BuildContractHtml(templateHtml, deal);
        }

        public async Task<string> GetContractPreviewForStartupAsync(int dealId, int startupId)
        {
            var deal = await _unitOfWork.Deals.GetByIdWithDetailsAsync(dealId)
                ?? throw new KeyNotFoundException("Deal not found.");

            EnsureStartupOwnsDeal(deal, startupId);
            EnsureDealAllowsContractPreview(deal);

            var templateHtml = await ReadContractTemplateAsync();
            return BuildContractHtml(templateHtml, deal);
        }

        public async Task<DealContractStatusResponse> InvestorSignContractAsync(int dealId, int investorId, InvestorSignContractDto request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var deal = await _unitOfWork.Deals.GetByIdWithDetailsAsync(dealId)
                ?? throw new KeyNotFoundException("Deal not found.");

            EnsureInvestorOwnsDeal(deal, investorId);

            if (deal.Status != DealStatus.Confirmed)
            {
                throw new InvalidOperationException("Only confirmed deals can be signed by investor.");
            }

            var (investorSignatureDataUri, _) = NormalizeAndDecodeSignature(request.SignatureBase64, "Investor signature");

            deal.Amount = request.FinalAmount;
            deal.EquityPercentage = (decimal)request.FinalEquityPercentage;
            deal.AdditionalTerms = request.AdditionalTerms?.Trim();
            deal.InvestorSignature = investorSignatureDataUri;
            deal.InvestorSignedAt = DateTime.UtcNow;
            deal.StartupSignature = null;
            deal.StartupSignedAt = null;
            deal.ContractPdfUrl = null;
            deal.Status = DealStatus.Waiting_For_Startup_Signature;

            _unitOfWork.Deals.Update(deal);
            await _unitOfWork.SaveChangesAsync();

            await _notificationService.SendNotificationAsync(
                deal.Project.Startup.UserId,
                "Nhà đầu tư đã ký hợp đồng",
                "Nhà đầu tư đã ký thỏa thuận. Vui lòng xem lại và ký để hoàn tất.",
                NotificationType.Deal,
                deal.DealId,
                "Deal");

            return _mapper.Map<DealContractStatusResponse>(deal);
        }

        public async Task<DealContractStatusResponse> StartupSignContractAsync(int dealId, int startupId, StartupSignContractDto request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var deal = await _unitOfWork.Deals.GetByIdWithDetailsAsync(dealId)
                ?? throw new KeyNotFoundException("Deal not found.");

            EnsureStartupOwnsDeal(deal, startupId);

            if (deal.Status != DealStatus.Waiting_For_Startup_Signature)
            {
                throw new InvalidOperationException("Deal is not waiting for startup signature.");
            }

            if (string.IsNullOrWhiteSpace(deal.InvestorSignature))
            {
                throw new InvalidOperationException("Investor must sign first.");
            }

            var (_, investorSignatureBytes) = NormalizeAndDecodeSignature(deal.InvestorSignature, "Investor signature");
            var (startupSignatureDataUri, startupSignatureBytes) = NormalizeAndDecodeSignature(request.SignatureBase64, "Startup signature");

            EnsureImageRenderableByPdfEngine(investorSignatureBytes, "Investor signature");
            EnsureImageRenderableByPdfEngine(startupSignatureBytes, "Startup signature");

            var investorWallet = GetInvestorWalletAddressOrThrow(deal);
            var registeredDocumentHash = await GetRegisteredProjectDocumentHashAsync(deal.ProjectId);

            deal.StartupSignature = startupSignatureDataUri;
            deal.StartupSignedAt = DateTime.UtcNow;

            var templateHtml = await ReadContractTemplateAsync();
            var finalizedHtml = BuildContractHtml(templateHtml, deal);

            byte[] pdfBytes;
            try
            {
                pdfBytes = GenerateContractPdf(deal, investorSignatureBytes, startupSignatureBytes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to generate contract PDF. {ex.Message}", ex);
            }

            string pdfPath;
            try
            {
                pdfPath = await UploadContractPdfAsync(pdfBytes, deal.DealId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to upload generated contract PDF.", ex);
            }

            deal.ContractPdfUrl = pdfPath;
            deal.Status = DealStatus.Contract_Signed;

            _unitOfWork.Deals.Update(deal);
            await _unitOfWork.SaveChangesAsync();

            await _notificationService.SendNotificationAsync(
                deal.Investor.UserId,
                "Hợp đồng đã hoàn tất",
                "Startup đã ký thỏa thuận. Hợp đồng hiện đã hoàn tất.",
                NotificationType.Deal,
                deal.DealId,
                "Deal");

            await QueueOrExecuteDocumentOwnerAssignmentAsync(
                deal.DealId,
                deal.ProjectId,
                registeredDocumentHash,
                investorWallet,
                deal.Investor.UserId);

            _ = finalizedHtml;
            return _mapper.Map<DealContractStatusResponse>(deal);
        }

        public async Task<DealContractStatusResponse> StartupRejectContractAsync(int dealId, int startupId, StartupRejectContractDto request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var deal = await _unitOfWork.Deals.GetByIdWithDetailsAsync(dealId)
                ?? throw new KeyNotFoundException("Deal not found.");

            EnsureStartupOwnsDeal(deal, startupId);

            if (deal.Status != DealStatus.Waiting_For_Startup_Signature)
            {
                throw new InvalidOperationException("Only deals waiting for startup signature can be rejected at this stage.");
            }

            deal.StartupSignature = null;
            deal.StartupSignedAt = null;
            deal.ContractPdfUrl = null;
            deal.Status = DealStatus.Rejected;

            _unitOfWork.Deals.Update(deal);
            await _unitOfWork.SaveChangesAsync();

            var startupReason = request.Reason.Trim();

            await _notificationService.SendNotificationAsync(
                deal.Investor.UserId,
                "Hợp đồng bị startup từ chối",
                $"Startup đã từ chối thỏa thuận. Lý do: {startupReason}",
                NotificationType.Deal,
                deal.DealId,
                "Deal");

            return _mapper.Map<DealContractStatusResponse>(deal);
        }

        public async Task<DealContractStatusResponse> GetContractStatusForInvestorAsync(int dealId, int investorId)
        {
            var deal = await _unitOfWork.Deals.GetByIdWithDetailsAsync(dealId)
                ?? throw new KeyNotFoundException("Deal not found.");

            EnsureInvestorOwnsDeal(deal, investorId);
            return _mapper.Map<DealContractStatusResponse>(deal);
        }

        public async Task<DealContractStatusResponse> GetContractStatusForStartupAsync(int dealId, int startupId)
        {
            var deal = await _unitOfWork.Deals.GetByIdWithDetailsAsync(dealId)
                ?? throw new KeyNotFoundException("Deal not found.");

            EnsureStartupOwnsDeal(deal, startupId);
            return _mapper.Map<DealContractStatusResponse>(deal);
        }

        public async Task<DealContractStatusResponse> GetContractStatusAsync(int dealId)
        {
            var deal = await _unitOfWork.Deals.GetByIdWithDetailsAsync(dealId)
                ?? throw new KeyNotFoundException("Deal not found.");

            return _mapper.Map<DealContractStatusResponse>(deal);
        }

        public async Task<DealOwnershipAssignmentStatusResponse> GetOwnershipAssignmentStatusForInvestorAsync(int dealId, int investorId)
        {
            var deal = await _unitOfWork.Deals.GetByIdWithDetailsAsync(dealId)
                ?? throw new KeyNotFoundException("Deal not found.");

            EnsureInvestorOwnsDeal(deal, investorId);
            return await BuildOwnershipAssignmentStatusAsync(deal);
        }

        public async Task<DealOwnershipAssignmentStatusResponse> GetOwnershipAssignmentStatusForStartupAsync(int dealId, int startupId)
        {
            var deal = await _unitOfWork.Deals.GetByIdWithDetailsAsync(dealId)
                ?? throw new KeyNotFoundException("Deal not found.");

            EnsureStartupOwnsDeal(deal, startupId);
            return await BuildOwnershipAssignmentStatusAsync(deal);
        }

        public async Task<DealOwnershipAssignmentStatusResponse> GetOwnershipAssignmentStatusAsync(int dealId)
        {
            var deal = await _unitOfWork.Deals.GetByIdWithDetailsAsync(dealId)
                ?? throw new KeyNotFoundException("Deal not found.");

            return await BuildOwnershipAssignmentStatusAsync(deal);
        }

        private async Task<DealOwnershipAssignmentStatusResponse> BuildOwnershipAssignmentStatusAsync(Deal deal)
        {
            var investorWallet = GetInvestorWalletAddressOrThrow(deal);

            var projectDocuments = await _unitOfWork.Documents.GetByProjectIdAsync(deal.ProjectId);
            var registeredDocument = projectDocuments
                .Where(d => !string.IsNullOrWhiteSpace(d.FileHash))
                .FirstOrDefault(d => !string.IsNullOrWhiteSpace(d.BlockchainTxHash));

            if (registeredDocument is null)
            {
                throw new InvalidOperationException("No registered project document hash found on blockchain for this deal.");
            }

            var documentHash = registeredDocument.FileHash!;
            var (startupIdOnChain, timestampOnChain, owners) = await _blockchainService.VerifyDocumentAsync(documentHash);

            var ownerList = owners.ToList();
            var isOwnerAssignedOnChain = ownerList.Any(owner =>
                string.Equals(owner, investorWallet, StringComparison.OrdinalIgnoreCase));

            return new DealOwnershipAssignmentStatusResponse
            {
                DealId = deal.DealId,
                ProjectId = deal.ProjectId,
                DocumentHash = documentHash,
                InvestorWallet = investorWallet,
                IsOwnerAssignedOnChain = isOwnerAssignedOnChain,
                RegisterDocumentTxHash = registeredDocument.BlockchainTxHash ?? string.Empty,
                TimestampOnBlockchain = timestampOnChain > 0
                    ? DateTimeOffset.FromUnixTimeSeconds(timestampOnChain).UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss UTC", CultureInfo.InvariantCulture)
                    : string.Empty,
                OnChainOwners = ownerList,
                Message = BuildOwnershipStatusMessage(isOwnerAssignedOnChain, startupIdOnChain, deal.ProjectId, timestampOnChain)
            };
        }

        private static string BuildOwnershipStatusMessage(
            bool isOwnerAssignedOnChain,
            int startupIdOnChain,
            int expectedProjectId,
            long timestampOnChain)
        {
            if (timestampOnChain <= 0)
            {
                return "Document hash is not registered on current contract. If this project/document was approved before changing ContractAddress, you need to register this hash again on the current contract.";
            }

            if (startupIdOnChain != expectedProjectId)
            {
                return "Blockchain document exists but startup/project id does not match this deal.";
            }

            return isOwnerAssignedOnChain
                ? "Investor wallet has been assigned as document owner on blockchain."
                : "Investor wallet is not in current on-chain owner list yet.";
        }

        private async Task<string> ReadContractTemplateAsync()
        {
            var templatePath = Path.Combine(_environment.ContentRootPath, "Templates", "ContractTemplate.html");
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException("Contract template was not found.", templatePath);
            }

            return await File.ReadAllTextAsync(templatePath);
        }

        private static string BuildContractHtml(string templateHtml, Deal deal)
        {
            var hasInvestorSignature = !string.IsNullOrWhiteSpace(deal.InvestorSignature);
            var hasStartupSignature = !string.IsNullOrWhiteSpace(deal.StartupSignature);

            var investorName = GetInvestorDisplayName(deal);
            var startupRepName = GetStartupRepresentativeName(deal);
            var investorEmail = deal.Investor.User?.Email ?? string.Empty;
            var startupEmail = deal.Project.Startup.User?.Email
                ?? deal.Project.Startup.Email
                ?? string.Empty;

            var finalAmountText = deal.Amount.ToString("F2", CultureInfo.InvariantCulture);
            var finalEquityText = deal.EquityPercentage?.ToString("F2", CultureInfo.InvariantCulture) ?? ".......";

            var investmentTermsBlock = hasInvestorSignature
                ? $"<h3>2. Investment Terms</h3><p><span class=\"label\">Final Amount (VND):</span> {finalAmountText}</p><p><span class=\"label\">Final Equity (%):</span> {finalEquityText}</p>"
                : "<h3>2. Investment Terms</h3><p><i>[Will be updated after investor finalizes amount and equity percentage]</i></p>";

            var additionalTermsBlock = hasInvestorSignature && !string.IsNullOrWhiteSpace(deal.AdditionalTerms)
                ? $"<h3>3. Additional Terms</h3><div class=\"box\">{deal.AdditionalTerms.Trim()}</div>"
                : "<h3>3. Additional Terms</h3><p><i>[No additional terms yet]</i></p>";

            var investorSignatureSection = BuildSignatureSection(
                "Investor Signature",
                investorName,
                hasInvestorSignature ? NormalizeSignatureDataUri(deal.InvestorSignature!) : null,
                deal.InvestorSignedAt);

            var startupSignatureSection = BuildSignatureSection(
                "Startup Representative",
                startupRepName,
                hasStartupSignature ? NormalizeSignatureDataUri(deal.StartupSignature!) : null,
                deal.StartupSignedAt);

            var replacements = new Dictionary<string, string>
            {
                ["{{DealId}}"] = deal.DealId.ToString(CultureInfo.InvariantCulture),
                ["{{ContractDate}}"] = deal.DealDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ["{{InvestorName}}"] = investorName,
                ["{{InvestorEmail}}"] = investorEmail,
                ["{{ProjectName}}"] = deal.Project.ProjectName,
                ["{{StartupRepName}}"] = startupRepName,
                ["{{StartupEmail}}"] = startupEmail,
                ["{{InvestmentTermsBlock}}"] = investmentTermsBlock,
                ["{{AdditionalTermsBlock}}"] = additionalTermsBlock,
                ["{{InvestorSignatureSection}}"] = investorSignatureSection,
                ["{{StartupSignatureSection}}"] = startupSignatureSection
            };

            var html = templateHtml;
            foreach (var item in replacements)
            {
                html = html.Replace(item.Key, item.Value, StringComparison.OrdinalIgnoreCase);
            }

            return html;
        }

        private static string BuildSignatureSection(string sectionLabel, string signerName, string? signatureDataUri, DateTime? signedAt)
        {
            if (string.IsNullOrWhiteSpace(signatureDataUri))
            {
                return $"<div class=\"line\">{signerName}</div>";
            }

            var signedAtRow = signedAt.HasValue
                ? $"<p>Signed At: {FormatSignedAt(signedAt)}</p>"
                : string.Empty;

            return $"<div class=\"label\">{sectionLabel}</div><div class=\"box\"><img class=\"signature-img\" src=\"{signatureDataUri}\" alt=\"{sectionLabel}\" /></div>{signedAtRow}<div class=\"line\">{signerName}</div>";
        }

        private static string NormalizeSignatureDataUri(string signatureBase64)
        {
            if (string.IsNullOrWhiteSpace(signatureBase64))
            {
                return string.Empty;
            }

            if (signatureBase64.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                return signatureBase64;
            }

            if (signatureBase64.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
            {
                return signatureBase64;
            }

            return $"data:image/png;base64,{signatureBase64}";
        }

        private static byte[]? TryExtractSignatureImageBytes(string signatureDataUri)
        {
            if (string.IsNullOrWhiteSpace(signatureDataUri))
            {
                return null;
            }

            var commaIndex = signatureDataUri.IndexOf(',');
            var base64 = commaIndex >= 0
                ? signatureDataUri[(commaIndex + 1)..]
                : signatureDataUri;

            base64 = NormalizeBase64Payload(base64);

            try
            {
                return Convert.FromBase64String(base64);
            }
            catch
            {
                return null;
            }
        }

        private static string NormalizeBase64Payload(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(value.Length);
            foreach (var c in value)
            {
                if (!char.IsWhiteSpace(c))
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }

        private static (string DataUri, byte[] Bytes) NormalizeAndDecodeSignature(string? signatureBase64, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(signatureBase64))
            {
                throw new InvalidOperationException($"{fieldName} is required.");
            }

            var dataUri = NormalizeSignatureDataUri(signatureBase64.Trim());
            if (!dataUri.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"{fieldName} must be an image in base64 format.");
            }

            var imageBytes = TryExtractSignatureImageBytes(dataUri)
                ?? throw new InvalidOperationException($"{fieldName} format is invalid.");

            if (!IsSupportedSignatureImage(imageBytes))
            {
                throw new InvalidOperationException($"{fieldName} must be PNG, JPEG, or WEBP.");
            }

            return (dataUri, imageBytes);
        }

        private static bool IsSupportedSignatureImage(byte[] bytes)
        {
            if (bytes.Length >= 8
                && bytes[0] == 0x89
                && bytes[1] == 0x50
                && bytes[2] == 0x4E
                && bytes[3] == 0x47
                && bytes[4] == 0x0D
                && bytes[5] == 0x0A
                && bytes[6] == 0x1A
                && bytes[7] == 0x0A)
            {
                return true;
            }

            if (bytes.Length >= 3
                && bytes[0] == 0xFF
                && bytes[1] == 0xD8
                && bytes[2] == 0xFF)
            {
                return true;
            }

            if (bytes.Length >= 12
                && bytes[0] == (byte)'R'
                && bytes[1] == (byte)'I'
                && bytes[2] == (byte)'F'
                && bytes[3] == (byte)'F'
                && bytes[8] == (byte)'W'
                && bytes[9] == (byte)'E'
                && bytes[10] == (byte)'B'
                && bytes[11] == (byte)'P')
            {
                return true;
            }

            return false;
        }

        private static void EnsureImageRenderableByPdfEngine(byte[] imageBytes, string fieldName)
        {
            try
            {
                var probeDocument = QuestPDF.Fluent.Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(10);
                        page.Content().Image(imageBytes);
                    });
                });

                _ = probeDocument.GeneratePdf();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"{fieldName} is not renderable by PDF engine. Please send a plain PNG/JPEG signature image. {ex.Message}", ex);
            }
        }

        private byte[] GenerateContractPdf(
            Deal deal,
            byte[] investorSignatureBytes,
            byte[] startupSignatureBytes)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var investorName = GetInvestorDisplayName(deal);
            var startupRepName = GetStartupRepresentativeName(deal);
            var investorEmail = deal.Investor.User?.Email ?? string.Empty;
            var startupEmail = deal.Project.Startup.User?.Email
                ?? deal.Project.Startup.Email
                ?? string.Empty;

            var contractDate = deal.DealDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var amountText = deal.Amount.ToString("F2", CultureInfo.InvariantCulture);
            var equityText = (deal.EquityPercentage ?? 0m).ToString("F2", CultureInfo.InvariantCulture);
            var termsText = string.IsNullOrWhiteSpace(deal.AdditionalTerms) ? "N/A" : deal.AdditionalTerms.Trim();

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Content().Column(column =>
                    {
                        column.Spacing(8);

                        column.Item().Text("AISEP Investment Contract").FontSize(20).SemiBold();
                        column.Item().Text($"Contract No: DEAL-{deal.DealId}").FontSize(11);
                        column.Item().Text($"Date: {contractDate}").FontSize(11);

                        column.Item().PaddingTop(8).Text("1. Parties").SemiBold();
                        column.Item().Text($"Investor: {investorName} (Email: {investorEmail})");
                        column.Item().Text($"Startup/Project: {deal.Project.ProjectName} - Rep: {startupRepName} (Email: {startupEmail})");

                        column.Item().PaddingTop(8).Text("2. Investment Terms").SemiBold();
                        column.Item().Text($"Final Amount (VND): {amountText}");
                        column.Item().Text($"Final Equity (%): {equityText}");

                        column.Item().PaddingTop(8).Text("3. Additional Terms").SemiBold();
                        column.Item().Border(1).Padding(8).Text(termsText);

                        column.Item().PaddingTop(12).Row(row =>
                        {
                            row.Spacing(20);

                            row.RelativeItem().Column(signColumn =>
                            {
                                signColumn.Spacing(4);
                                signColumn.Item().Text("Investor Signature").SemiBold();
                                signColumn.Item().Border(1).Padding(8).MinHeight(90).MaxHeight(120).AlignMiddle().AlignCenter().Image(investorSignatureBytes).FitArea();
                                signColumn.Item().Text($"Signed At: {FormatSignedAt(deal.InvestorSignedAt)}").FontSize(10);
                                signColumn.Item().Text(investorName).FontSize(10);
                            });

                            row.RelativeItem().Column(signColumn =>
                            {
                                signColumn.Spacing(4);
                                signColumn.Item().Text("Startup Representative").SemiBold();
                                signColumn.Item().Border(1).Padding(8).MinHeight(90).MaxHeight(120).AlignMiddle().AlignCenter().Image(startupSignatureBytes).FitArea();
                                signColumn.Item().Text($"Signed At: {FormatSignedAt(deal.StartupSignedAt)}").FontSize(10);
                                signColumn.Item().Text(startupRepName).FontSize(10);
                            });
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }

        private async Task<string> UploadContractPdfAsync(byte[] pdfBytes, int dealId)
        {
            await using var stream = new MemoryStream(pdfBytes);
            IFormFile contractFile = new FormFile(stream, 0, stream.Length, $"deal_{dealId}", $"deal-{dealId}-contract.pdf")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf"
            };

            return await _storageService.UploadFileAsync(contractFile, "deal-contracts");
        }

        private static string GetInvestorWalletAddressOrThrow(Deal deal)
        {
            var walletAddress = deal.Investor.WalletAddress?.Trim();
            if (string.IsNullOrWhiteSpace(walletAddress))
            {
                throw new InvalidOperationException(
                    "Investor wallet address is missing. Please update investor wallet before completing contract signing.");
            }

            return walletAddress;
        }

        private async Task<string> GetRegisteredProjectDocumentHashAsync(int projectId)
        {
            var projectDocuments = await _unitOfWork.Documents.GetByProjectIdAsync(projectId);

            var registeredDocument = projectDocuments
                .Where(d => !string.IsNullOrWhiteSpace(d.FileHash))
                .FirstOrDefault(d => !string.IsNullOrWhiteSpace(d.BlockchainTxHash));

            if (registeredDocument is null)
            {
                throw new InvalidOperationException(
                    "No registered project document hash found on blockchain for this deal.");
            }

            return registeredDocument.FileHash!;
        }

        private async Task QueueOrExecuteDocumentOwnerAssignmentAsync(
            int dealId,
            int projectId,
            string documentHash,
            string investorWallet,
            int investorUserId)
        {
            var workItem = new DocumentOwnerAssignmentWorkItem(
                dealId,
                projectId,
                documentHash,
                investorWallet,
                investorUserId);

            try
            {
                await _blockchainOwnershipAssignmentQueue.QueueAsync(workItem);
            }
            catch (Exception queueEx)
            {
                _logger.LogWarning(
                    queueEx,
                    "Failed to enqueue blockchain owner assignment for DealId {DealId}. Fallback to direct call.",
                    dealId);

                try
                {
                    var txHash = await _blockchainService.AssignDocumentOwnerAsync(documentHash, investorWallet);

                    await _notificationService.SendNotificationAsync(
                        investorUserId,
                        "Đã chuyển giao quyền sở hữu tài liệu",
                        $"Thỏa thuận #{dealId} đã được ghi nhận chuyển giao quyền sở hữu tài liệu trên Blockchain cho ví {investorWallet}. Mã giao dịch: {txHash}",
                        NotificationType.Deal,
                        dealId,
                        "Deal");

                    _logger.LogInformation(
                        "Fallback blockchain owner assignment succeeded for DealId {DealId}. TxHash: {TxHash}",
                        dealId,
                        txHash);
                }
                catch (Exception fallbackEx)
                {
                    _logger.LogError(
                        fallbackEx,
                        "Fallback blockchain owner assignment failed for DealId {DealId}, ProjectId {ProjectId}.",
                        dealId,
                        projectId);
                }
            }
        }

        private static void EnsureDealAllowsContractPreview(Deal deal)
        {
            if (deal.Status != DealStatus.Confirmed
                && deal.Status != DealStatus.Waiting_For_Startup_Signature
                && deal.Status != DealStatus.Contract_Signed)
            {
                throw new InvalidOperationException("Contract preview is only available for deals in signing flow.");
            }
        }

        private static string GetInvestorDisplayName(Deal deal)
        {
            if (!string.IsNullOrWhiteSpace(deal.Investor.OrganizationName))
            {
                return deal.Investor.OrganizationName;
            }

            if (!string.IsNullOrWhiteSpace(deal.Investor.User?.FullName))
            {
                return deal.Investor.User.FullName;
            }

            if (!string.IsNullOrWhiteSpace(deal.Investor.User?.UserName))
            {
                return deal.Investor.User.UserName;
            }

            return $"Investor #{deal.InvestorId}";
        }

        private static string GetStartupRepresentativeName(Deal deal)
        {
            if (!string.IsNullOrWhiteSpace(deal.Project.Startup.User?.FullName))
            {
                return deal.Project.Startup.User.FullName;
            }

            if (!string.IsNullOrWhiteSpace(deal.Project.Startup.User?.UserName))
            {
                return deal.Project.Startup.User.UserName;
            }

            if (!string.IsNullOrWhiteSpace(deal.Project.Startup.CompanyName))
            {
                return deal.Project.Startup.CompanyName;
            }

            return $"Startup #{deal.Project.StartupId}";
        }

        private static string FormatSignedAt(DateTime? signedAt)
        {
            return signedAt.HasValue
                ? signedAt.Value.ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture)
                : string.Empty;
        }

        private static void EnsureInvestorOwnsDeal(Deal deal, int investorId)
        {
            if (deal.InvestorId != investorId)
            {
                throw new ForbiddenAccessException("You do not have permission to access this deal.");
            }
        }

        private static void EnsureStartupOwnsDeal(Deal deal, int startupId)
        {
            if (deal.Project.StartupId != startupId)
            {
                throw new ForbiddenAccessException("You do not have permission to access this deal.");
            }
        }

    }
}

