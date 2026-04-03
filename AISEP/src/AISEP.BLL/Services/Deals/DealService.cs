using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Exceptions;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Blockchain;
using AISEP.BLL.Services.Notifications;
using AISEP.BLL.Services.Pinata;
using AISEP.BLL.Services.Storage;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Sieve.Models;
using Sieve.Services;
using System.Globalization;

namespace AISEP.BLL.Services.Deals
{
    public class DealService : IDealService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IBlockchainService _blockchainService;
        private readonly IPinataService _pinataService;
        private readonly IStorageService _storageService;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IWebHostEnvironment _environment;

        public DealService(
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            IMapper mapper,
            IConfiguration configuration,
            IBlockchainService blockchainService,
            IPinataService pinataService,
            IStorageService storageService,
            ISieveProcessor sieveProcessor,
            IWebHostEnvironment environment)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _mapper = mapper;
            _configuration = configuration;
            _blockchainService = blockchainService;
            _pinataService = pinataService;
            _storageService = storageService;
            _sieveProcessor = sieveProcessor;
            _environment = environment;
        }

        public async Task<DealDto> CreateDealAsync(int investorId, CreateDealDto dto)
        {
            if (dto.ProjectId <= 0)
            {
                throw new InvalidOperationException("ProjectId must be greater than 0.");
            }

            var investor = await _unitOfWork.Investors.GetByIdAsync(investorId)
                ?? throw new KeyNotFoundException("Investor not found.");

            var project = await _unitOfWork.Projects.GetByIdAsync(dto.ProjectId)
                ?? throw new KeyNotFoundException("Project not found.");

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
                "New deal proposal",
                $"A new deal was proposed for project '{project.ProjectName}'.",
                NotificationType.Deal);

            var created = await _unitOfWork.Deals.GetByIdWithNftAsync(deal.DealId)
                ?? throw new KeyNotFoundException("Created deal not found.");

            return _mapper.Map<DealDto>(created);
        }

        public async Task<DealDto> ConfirmDealAsync(int startupId, int dealId)
        {
            var deal = await _unitOfWork.Deals.GetByIdWithNftAsync(dealId)
                ?? throw new KeyNotFoundException("Deal not found.");

            if (deal.Project.StartupId != startupId)
            {
                throw new ForbiddenAccessException("You do not have permission to confirm this deal.");
            }

            if (deal.Status != DealStatus.Pending)
            {
                throw new InvalidOperationException("Only pending deals can be confirmed.");
            }

            deal.StartupConfirmed = true;
            deal.Status = DealStatus.Confirmed;
            _unitOfWork.Deals.Update(deal);
            await _unitOfWork.SaveChangesAsync();

            await _notificationService.SendNotificationAsync(
                deal.Investor.UserId,
                "Deal confirmed",
                $"Your deal #{deal.DealId} has been confirmed by the startup.",
                NotificationType.Deal);

            return _mapper.Map<DealDto>(deal);
        }

        public async Task<string> GetContractPreviewAsync(int dealId, int investorId)
        {
            var deal = await _unitOfWork.Deals.GetByIdWithNftAsync(dealId)
                ?? throw new KeyNotFoundException("Deal not found.");

            EnsureInvestorOwnsDeal(deal, investorId);

            if (deal.Status != DealStatus.Confirmed && deal.Status != DealStatus.Contract_Signed && deal.Status != DealStatus.Minted_NFT)
            {
                throw new InvalidOperationException("Contract preview is only available for confirmed deals.");
            }

            var templateHtml = await ReadContractTemplateAsync();

            var finalEquity = deal.EquityPercentage.HasValue
                ? (double)deal.EquityPercentage.Value
                : 0d;

            return BuildContractHtml(
                templateHtml,
                deal,
                deal.Amount,
                finalEquity,
                string.Empty,
                string.Empty);
        }

        public async Task<DealContractStatusResponse> SignAndFinalizeContractAsync(int dealId, int investorId, int signedByUserId, SignContractRequestDto request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.FinalAmount <= 0)
            {
                throw new InvalidOperationException("FinalAmount must be greater than 0.");
            }

            if (request.FinalEquityPercentage < 0)
            {
                throw new InvalidOperationException("FinalEquityPercentage must be greater than or equal to 0.");
            }

            if (string.IsNullOrWhiteSpace(request.SignatureBase64))
            {
                throw new InvalidOperationException("SignatureBase64 is required.");
            }

            var deal = await _unitOfWork.Deals.GetByIdWithNftAsync(dealId)
                ?? throw new KeyNotFoundException("Deal not found.");

            EnsureInvestorOwnsDeal(deal, investorId);

            if (deal.Status != DealStatus.Confirmed)
            {
                throw new InvalidOperationException("Only confirmed deals can be signed.");
            }

            deal.Amount = request.FinalAmount;
            deal.EquityPercentage = (decimal)request.FinalEquityPercentage;

            var templateHtml = await ReadContractTemplateAsync();
            var signatureImageData = NormalizeSignatureDataUri(request.SignatureBase64);
            var signatureImageBytes = TryExtractSignatureImageBytes(signatureImageData)
                ?? throw new InvalidOperationException("SignatureBase64 format is invalid.");

            var finalizedHtml = BuildContractHtml(
                templateHtml,
                deal,
                request.FinalAmount,
                request.FinalEquityPercentage,
                request.AdditionalTerms,
                $"<img src=\"{signatureImageData}\" alt=\"signature\" style=\"max-height:80px;\" />");

            var pdfBytes = GenerateContractPdf(
                deal,
                request.FinalAmount,
                request.FinalEquityPercentage,
                request.AdditionalTerms,
                signatureImageBytes);

            var pdfPath = await UploadContractPdfAsync(pdfBytes, deal.DealId);

            deal.Status = DealStatus.Contract_Signed;
            deal.ContractPdfUrl = pdfPath;
            deal.ContractSignedAt = DateTime.UtcNow;
            deal.ContractSignedByUserId = signedByUserId;
            _unitOfWork.Deals.Update(deal);
            await _unitOfWork.SaveChangesAsync();

            _ = finalizedHtml;
            return _mapper.Map<DealContractStatusResponse>(deal);
        }

        public async Task<DealContractStatusResponse> GetContractStatusForInvestorAsync(int dealId, int investorId)
        {
            var deal = await _unitOfWork.Deals.GetByIdWithNftAsync(dealId)
                ?? throw new KeyNotFoundException("Deal not found.");

            EnsureInvestorOwnsDeal(deal, investorId);
            return _mapper.Map<DealContractStatusResponse>(deal);
        }

        public async Task<DealContractStatusResponse> GetContractStatusForStartupAsync(int dealId, int startupId)
        {
            var deal = await _unitOfWork.Deals.GetByIdWithNftAsync(dealId)
                ?? throw new KeyNotFoundException("Deal not found.");

            EnsureStartupOwnsDeal(deal, startupId);
            return _mapper.Map<DealContractStatusResponse>(deal);
        }

        public async Task<DealDto> MintNftForDealAsync(int dealId, MintNftRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.OwnerWallet))
            {
                throw new InvalidOperationException("Owner wallet is required.");
            }

            var deal = await _unitOfWork.Deals.GetByIdWithNftAsync(dealId)
                ?? throw new KeyNotFoundException("Deal not found.");

            if (deal.Status != DealStatus.Confirmed && deal.Status != DealStatus.Contract_Signed)
            {
                throw new InvalidOperationException("Only confirmed or contract-signed deals can mint NFT.");
            }

            if (deal.NFTRecord is not null)
            {
                throw new InvalidOperationException("NFT has already been minted for this deal.");
            }

            var imageUrl = _configuration["Pinata:NftTemplateImage"];
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                throw new InvalidOperationException("Pinata:NftTemplateImage is missing in configuration.");
            }

            if (!imageUrl.StartsWith("ipfs://", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Pinata:NftTemplateImage must start with 'ipfs://'.");
            }

            var investorName = string.IsNullOrWhiteSpace(deal.Investor.OrganizationName)
                ? $"Investor #{deal.InvestorId}"
                : deal.Investor.OrganizationName;

            var metadata = new NftMetadataDto
            {
                Name = $"AISEP Investment Certificate #{deal.DealId}",
                Description = $"NFT certificate for investment in project '{deal.Project.ProjectName}'.",
                Image = imageUrl,
                Attributes =
                [
                    new NftAttributeDto { TraitType = "Project Name", Value = deal.Project.ProjectName },
                    new NftAttributeDto { TraitType = "Investor Name", Value = investorName },
                    new NftAttributeDto
                    {
                        TraitType = "Investment Amount",
                        Value = deal.Amount.ToString("F2", CultureInfo.InvariantCulture)
                    }
                ]
            };

            var ipfsHash = await _pinataService.UploadJsonToIpfsAsync(metadata);
            var metadataUri = $"ipfs://{ipfsHash}";
            var (tokenId, txHash) = await _blockchainService.MintCertificateAsync(request.OwnerWallet, metadataUri);

            var nftRecord = new NFTRecord
            {
                DealId = deal.DealId,
                TokenId = tokenId,
                TxHash = txHash,
                OwnerWallet = request.OwnerWallet,
                MintedAt = DateTime.UtcNow,
                ValidityStatus = ValidityStatus.Valid,
                Transferable = false
            };

            await _unitOfWork.NFTRecords.AddAsync(nftRecord);

            deal.Status = DealStatus.Minted_NFT;
            deal.IsCompleted = true;
            deal.CompletionDate = DateTime.UtcNow;
            _unitOfWork.Deals.Update(deal);

            await _unitOfWork.SaveChangesAsync();

            var updated = await _unitOfWork.Deals.GetByIdWithNftAsync(deal.DealId)
                ?? throw new KeyNotFoundException("Minted deal not found.");

            return _mapper.Map<DealDto>(updated);
        }

        public async Task<PagedResult<DealDto>> GetMyNftsAsync(int investorId, SieveModel sieveModel)
        {
            sieveModel ??= new SieveModel();

            var query = _unitOfWork.Deals.GetQuery()
                .Where(d => d.InvestorId == investorId && d.Status == DealStatus.Minted_NFT);

            return await PaginationHelper.PaginateAsync(
                query,
                sieveModel,
                _sieveProcessor,
                d => _mapper.Map<DealDto>(d));
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

        private static string BuildContractHtml(
            string templateHtml,
            Deal deal,
            decimal finalAmount,
            double finalEquityPercentage,
            string? additionalTerms,
            string signatureImageHtml)
        {
            var investorName = string.IsNullOrWhiteSpace(deal.Investor.OrganizationName)
                ? $"Investor #{deal.InvestorId}"
                : deal.Investor.OrganizationName;

            var replacements = new Dictionary<string, string>
            {
                ["{{DealId}}"] = deal.DealId.ToString(CultureInfo.InvariantCulture),
                ["{{ProjectName}}"] = deal.Project.ProjectName,
                ["{{InvestorName}}"] = investorName,
                ["{{ProjectId}}"] = deal.ProjectId.ToString(CultureInfo.InvariantCulture),
                ["{{ContractDate}}"] = DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ["{{FinalAmount}}"] = finalAmount.ToString("F2", CultureInfo.InvariantCulture),
                ["{{FinalEquityPercentage}}"] = finalEquityPercentage.ToString("F2", CultureInfo.InvariantCulture),
                ["{{AdditionalTerms}}"] = string.IsNullOrWhiteSpace(additionalTerms) ? "N/A" : additionalTerms,
                ["{{SignatureImage}}"] = string.IsNullOrWhiteSpace(signatureImageHtml)
                    ? "<div style=\"height:80px;border:1px dashed #999;\"></div>"
                    : signatureImageHtml
            };

            var html = templateHtml;
            foreach (var item in replacements)
            {
                html = html.Replace(item.Key, item.Value, StringComparison.OrdinalIgnoreCase);
            }

            return html;
        }

        private static string NormalizeSignatureDataUri(string signatureBase64)
        {
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

            try
            {
                return Convert.FromBase64String(base64);
            }
            catch
            {
                return null;
            }
        }

        private byte[] GenerateContractPdf(
            Deal deal,
            decimal finalAmount,
            double finalEquityPercentage,
            string? additionalTerms,
            byte[] signatureImageBytes)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var investorName = string.IsNullOrWhiteSpace(deal.Investor.OrganizationName)
                ? $"Investor #{deal.InvestorId}"
                : deal.Investor.OrganizationName;

            var contractDate = DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var amountText = finalAmount.ToString("F2", CultureInfo.InvariantCulture);
            var equityText = finalEquityPercentage.ToString("F2", CultureInfo.InvariantCulture);
            var termsText = string.IsNullOrWhiteSpace(additionalTerms) ? "N/A" : additionalTerms.Trim();

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Content().Column(column =>
                    {
                        column.Spacing(8);

                        column.Item().Text("Investment Contract").FontSize(20).SemiBold();
                        column.Item().Text($"Contract No: DEAL-{deal.DealId}").FontSize(11);
                        column.Item().Text($"Date: {contractDate}").FontSize(11);

                        column.Item().PaddingTop(8).Text(
                            $"This contract confirms the investment agreement between {investorName} and project {deal.Project.ProjectName} (Project ID: {deal.ProjectId}).");

                        column.Item().PaddingTop(8).Text($"Final Amount (USD): {amountText}");
                        column.Item().Text($"Final Equity (%): {equityText}");

                        column.Item().PaddingTop(8).Text("Additional Terms").SemiBold();
                        column.Item().Border(1).Padding(8).Text(termsText);

                        column.Item().PaddingTop(12).Text("Investor Signature").SemiBold();
                        column.Item().Border(1).Padding(8).Height(90).AlignMiddle().AlignLeft().Image(signatureImageBytes);
                        column.Item().Text(investorName).FontSize(10);
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
