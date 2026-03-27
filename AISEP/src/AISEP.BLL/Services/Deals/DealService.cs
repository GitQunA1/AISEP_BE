using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Exceptions;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Blockchain;
using AISEP.BLL.Services.Notifications;
using AISEP.BLL.Services.Pinata;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AutoMapper;
using Microsoft.Extensions.Configuration;
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
        private readonly ISieveProcessor _sieveProcessor;

        public DealService(
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            IMapper mapper,
            IConfiguration configuration,
            IBlockchainService blockchainService,
            IPinataService pinataService,
            ISieveProcessor sieveProcessor)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _mapper = mapper;
            _configuration = configuration;
            _blockchainService = blockchainService;
            _pinataService = pinataService;
            _sieveProcessor = sieveProcessor;
        }

        public async Task<DealDto> CreateDealAsync(int investorId, CreateDealDto dto)
        {
            var investor = await _unitOfWork.Investors.GetByIdAsync(investorId)
                ?? throw new KeyNotFoundException("Investor not found.");

            var project = await _unitOfWork.Projects.GetByIdAsync(dto.ProjectId)
                ?? throw new KeyNotFoundException("Project not found.");

            var deal = new Deal
            {
                InvestorId = investorId,
                ProjectId = dto.ProjectId,
                Amount = dto.Amount,
                PaymentMethod = dto.PaymentMethod,
                EquityPercentage = dto.EquityPercentage,
                InvestorConfirmed = true,
                StartupConfirmed = false,
                Status = DealStatus.Pending,
                DealDate = DateTime.UtcNow,
                IsCompleted = false
            };

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

        public async Task<DealDto> MintNftForDealAsync(int dealId, MintNftRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.OwnerWallet))
            {
                throw new InvalidOperationException("Owner wallet is required.");
            }

            var deal = await _unitOfWork.Deals.GetByIdWithNftAsync(dealId)
                ?? throw new KeyNotFoundException("Deal not found.");

            if (deal.Status != DealStatus.Confirmed)
            {
                throw new InvalidOperationException("Only confirmed deals can mint NFT.");
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

    }
}
