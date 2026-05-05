using AISEP.BLL.Helpers;
using AISEP.DAL.Common;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AISEP.DAL.Repositories.Investors;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;
using AISEP.BLL.Services.Users;
using AISEP.BLL.Services.Notifications;
using AISEP.BLL.Services.FormValidationRules;
using AISEP.BLL.Services.Storage;
using AISEP.BLL.Exceptions;

namespace AISEP.BLL.Services.Investors
{
    public class InvestorService : IInvestorService
    {
        private const int MaxInvestorIndustries = 4;

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;
        private readonly IDynamicFormSubmissionValidationService _dynamicFormValidationService;
        private readonly IStorageService _storage;

        public InvestorService(IUnitOfWork unitOfWork, ISieveProcessor sieveProcessor, IMapper mapper, IUserService userService, INotificationService notificationService, IDynamicFormSubmissionValidationService dynamicFormValidationService, IStorageService storage)
        {
            _unitOfWork = unitOfWork;
            _sieveProcessor = sieveProcessor;
            _mapper = mapper;
            _userService = userService;
            _notificationService = notificationService;
            _dynamicFormValidationService = dynamicFormValidationService;
            _storage = storage;
        }

        public async Task<PagedResult<InvestorResponse>> GetAllAsync(SieveModel model)
        {
            var query = _unitOfWork.Investors.GetAllQuery();
            return await PaginationHelper.PaginateAsync(query, model, _sieveProcessor, i => _mapper.Map<InvestorResponse>(i));
        }

        public async Task<PagedResult<InvestorResponse>> GetMatchingInvestorsForCurrentStartupAsync(SieveModel model)
        {
            var currentUserId = _userService.GetUserId();
            var startup = await _unitOfWork.Startups.GetByUserIdAsync(currentUserId)
                ?? throw new KeyNotFoundException("Startup profile not found.");

            if (startup.ApprovalStatus != ApprovalStatus.Approved)
            {
                throw new InvalidOperationException("Your startup profile must be approved before using this feature.");
            }

            var industryIds = startup.StartupIndustries
                .Select(si => si.IndustryOptionId)
                .Concat(startup.Projects.Select(p => p.IndustryOptionId))
                .Where(id => id > 0)
                .ToHashSet();

            var stageOptionIds = startup.Projects
                .Where(p => p.Status == ProjectStatus.Approved && p.StageOptionId.HasValue)
                .Select(p => p.StageOptionId!.Value)
                .ToHashSet();

            var query = _unitOfWork.Investors.GetAllQuery()
                .Where(i => i.ApprovalStatus == ApprovalStatus.Approved)
                .OrderByDescending(i => i.InvestorIndustries.Any(ii => industryIds.Contains(ii.IndustryOptionId))
                    && i.PreferredStageOptionId.HasValue
                    && stageOptionIds.Contains(i.PreferredStageOptionId.Value))
                .ThenByDescending(i => i.InvestorIndustries.Any(ii => industryIds.Contains(ii.IndustryOptionId)))
                .ThenByDescending(i => i.PreferredStageOptionId.HasValue && stageOptionIds.Contains(i.PreferredStageOptionId.Value))
                .ThenBy(i => i.InvestorId);

            return await PaginationHelper.PaginateAsync(query, model, _sieveProcessor, i => _mapper.Map<InvestorResponse>(i));
        }

        public async Task<InvestorResponse?> GetByIdAsync(int investorId)
        {
            var investor = await _unitOfWork.Investors.GetByIdAsync(investorId);
            if (investor is null)
                throw new KeyNotFoundException("Investor not found.");
            return _mapper.Map<InvestorResponse>(investor);
        }

        public async Task<InvestorResponse?> GetMyProfileAsync()
        {
            var userId = _userService.GetUserId();
            var investor = await _unitOfWork.Investors.GetByUserIdAsync(userId);
            if (investor is null)
                throw new KeyNotFoundException("Investor profile not found.");
            return _mapper.Map<InvestorResponse>(investor);
        }

        public async Task<InvestorResponse?> CreateAsync(CreateInvestorRequest dto)
        {
            
            await _dynamicFormValidationService.ValidateAsync("investor.create", dto);

            var userId = _userService.GetUserId();
            var existing = await _unitOfWork.Investors.GetByUserIdAsync(userId);
            if (existing is not null)
                throw new InvalidOperationException("Investor profile already exists for this account.");
            var industryOptions = await ResolveIndustryOptionsAsync(dto.IndustryOptionIds);
            var preferredStageOption = await ResolveStageOptionAsync(dto.PreferredStageOptionId);

            var investor = _mapper.Map<Investor>(dto);
            investor.UserId = userId;
            investor.PreferredStageOptionId = preferredStageOption?.Id;
            investor.PreferredStageOption = preferredStageOption;
            investor.ProfileImageUrl = await UploadIfPresent(dto.ProfileImageFile, "investor-profiles");
            investor.ApprovalStatus = ApprovalStatus.Pending;
            investor.InvestorIndustries = industryOptions
                .Select(option => new InvestorIndustry
                {
                    IndustryOptionId = option.Id
                })
                .ToList();
            await _unitOfWork.Investors.AddAsync(investor);
            await _unitOfWork.SaveChangesAsync();
            await NotifyStaffAndAdminsAsync(
                "Hồ sơ nhà đầu tư chờ duyệt",
                "Có hồ sơ nhà đầu tư mới đã được gửi và đang chờ phê duyệt.",
                investor.InvestorId,
                "Investor");

            var created = await _unitOfWork.Investors.GetByIdAsync(investor.InvestorId);
            return _mapper.Map<InvestorResponse>(created!);
        }

        public async Task<InvestorResponse?> UpdateAsync(int id, UpdateInvestorRequest dto)
        {
            
            await _dynamicFormValidationService.ValidateAsync("investor.update", dto);

            var userId = _userService.GetUserId();

            var investor = await _unitOfWork.Investors.GetByIdAsync(id);
            if (investor is null)
                throw new KeyNotFoundException("Investor profile not found.");
            if (investor.UserId != userId)
                throw new ForbiddenAccessException("You do not have permission to update this investor.");
            if (investor.ApprovalStatus == ApprovalStatus.Pending)
                throw new InvalidOperationException("Your investor profile is already pending approval.");

            if (dto.OrganizationName is not null)
                investor.OrganizationName = dto.OrganizationName.Trim();

            if (dto.InvestmentTaste is not null)
                investor.InvestmentTaste = dto.InvestmentTaste.Trim();

            if (dto.WalletAddress is not null)
                investor.WalletAddress = dto.WalletAddress.Trim();

            if (dto.InvestmentAmount.HasValue)
                investor.InvestmentAmount = dto.InvestmentAmount.Value;

            if (dto.InvestmentDate.HasValue)
                investor.InvestmentDate = dto.InvestmentDate.Value;

            if (dto.RiskTolerance.HasValue)
                investor.RiskTolerance = dto.RiskTolerance.Value;

            if (dto.InvestmentRegion is not null)
                investor.InvestmentRegion = dto.InvestmentRegion.Trim();

            if (dto.IndustryOptionIds is not null)
            {
                var industryOptions = await ResolveIndustryOptionsAsync(dto.IndustryOptionIds);
                SyncInvestorIndustries(investor, industryOptions.Select(x => x.Id));
            }

            if (dto.PreferredStageOptionId.HasValue)
            {
                var preferredStageOption = await ResolveStageOptionAsync(dto.PreferredStageOptionId);
                investor.PreferredStageOptionId = preferredStageOption?.Id;
                investor.PreferredStageOption = preferredStageOption;
            }

            if (dto.PreviousInvestments is not null)
                investor.PreviousInvestments = dto.PreviousInvestments.Trim();

            if (dto.ProfileImageFile is not null)
                investor.ProfileImageUrl = await _storage.UploadFileAsync(dto.ProfileImageFile, "investor-profiles");

            investor.ApprovalStatus = ApprovalStatus.Pending;
            investor.ApprovedAt = null;
            investor.ApprovedById = null;
            investor.RejectedAt = null;
            investor.RejectedById = null;
            investor.RejectionReason = null;

            _unitOfWork.Investors.Update(investor);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<InvestorResponse>(investor);
        }

        public async Task ApproveInvestorAsync(int investorId)
        {
            var userId = _userService.GetUserId();
            var investor = await _unitOfWork.Investors.GetByIdAsync(investorId);
            if (investor is null)
                throw new KeyNotFoundException("Investor not found.");
            if (investor.ApprovalStatus == ApprovalStatus.Approved)
                throw new InvalidOperationException("Investor is already approved.");
            if (investor.ApprovalStatus != ApprovalStatus.Pending)
                throw new InvalidOperationException($"Only Pending investors can be approved. Current status: {investor.ApprovalStatus}.");

            investor.ApprovalStatus = ApprovalStatus.Approved;
            investor.ApprovedAt     = DateTime.UtcNow;
            investor.ApprovedById   = userId;

            _unitOfWork.Investors.Update(investor);
            await _unitOfWork.SaveChangesAsync();
            await _notificationService.SendNotificationAsync(
                investor.UserId,
                "Hồ sơ nhà đầu tư đã được duyệt",
                "Hồ sơ nhà đầu tư của bạn đã được duyệt.",
                NotificationType.General,
                investor.InvestorId,
                "Investor");
        }

        public async Task RejectInvestorAsync(int investorId, string rejectionReason)
        {
            var userId = _userService.GetUserId();
            var investor = await _unitOfWork.Investors.GetByIdAsync(investorId);
            if (investor is null)
                throw new KeyNotFoundException("Investor not found.");
            if (investor.ApprovalStatus == ApprovalStatus.Rejected)
                throw new InvalidOperationException("Investor is already rejected.");
            if (investor.ApprovalStatus != ApprovalStatus.Pending)
                throw new InvalidOperationException($"Only Pending investors can be rejected. Current status: {investor.ApprovalStatus}.");

            investor.ApprovalStatus  = ApprovalStatus.Rejected;
            investor.RejectedAt      = DateTime.UtcNow;
            investor.RejectedById    = userId;
            investor.RejectionReason = rejectionReason;

            _unitOfWork.Investors.Update(investor);
            await _unitOfWork.SaveChangesAsync();
            await _notificationService.SendNotificationAsync(
                investor.UserId,
                "Hồ sơ nhà đầu tư bị từ chối",
                $"Hồ sơ nhà đầu tư của bạn đã bị từ chối. Lý do: {rejectionReason}",
                NotificationType.General,
                investor.InvestorId,
                "Investor");
        }

        private async Task NotifyStaffAndAdminsAsync(string title, string message, int? referenceId = null, string? referenceType = null)
        {
            var reviewerIds = await _unitOfWork.Users.GetAllQuery()
                .Where(u => u.Role == UserRole.Staff || u.Role == UserRole.Admin)
                .Select(u => u.Id)
                .ToListAsync();

            foreach (var reviewerId in reviewerIds)
            {
                await _notificationService.SendNotificationAsync(
                    reviewerId,
                    title,
                    message,
                    NotificationType.System,
                    referenceId,
                    referenceType);
            }
        }

        private async Task<string?> UploadIfPresent(IFormFile? file, string folder)
            => file is not null ? await _storage.UploadFileAsync(file, folder) : null;

        // Kiểm tra danh sách ngành nhà đầu tư gửi lên có tồn tại và đang active hay không.
        private async Task<List<IndustryOption>> ResolveIndustryOptionsAsync(IEnumerable<int>? optionIds)
        {
            var ids = optionIds?
                .Distinct()
                .ToList() ?? [];

            if (ids.Count == 0)
            {
                throw new InvalidOperationException("At least one industry is required.");
            }
            if (ids.Count > MaxInvestorIndustries)
            {
                throw new InvalidOperationException($"Investor can select up to {MaxInvestorIndustries} industries.");
            }

            var options = await _unitOfWork.IndustryOptions.GetByIdsAsync(ids);
            if (options.Count != ids.Count || options.Any(x => !x.IsActive))
            {
                throw new InvalidOperationException("One or more selected industries are invalid or inactive.");
            }

            return options;
        }

        // Kiểm tra preferred stage của investor có tồn tại và đang active hay không.
        private async Task<StageOption?> ResolveStageOptionAsync(int? stageOptionId)
        {
            if (!stageOptionId.HasValue)
            {
                return null;
            }

            var option = await _unitOfWork.StageOptions.GetByIdAsync(stageOptionId.Value);
            if (option is null || !option.IsActive)
            {
                throw new InvalidOperationException("Selected preferred stage is invalid or inactive.");
            }

            return option;
        }

        // Đồng bộ bảng nối investor_industries với danh sách ngành mới nhất.
        private static void SyncInvestorIndustries(Investor investor, IEnumerable<int> industryOptionIds)
        {
            var requestedIds = industryOptionIds.ToHashSet();
            if (requestedIds.Count == 0)
            {
                throw new InvalidOperationException("At least one industry is required.");
            }
            if (requestedIds.Count > MaxInvestorIndustries)
            {
                throw new InvalidOperationException($"Investor can select up to {MaxInvestorIndustries} industries.");
            }

            var toRemove = investor.InvestorIndustries
                .Where(x => !requestedIds.Contains(x.IndustryOptionId))
                .ToList();

            foreach (var item in toRemove)
            {
                investor.InvestorIndustries.Remove(item);
            }

            var currentIds = investor.InvestorIndustries
                .Select(x => x.IndustryOptionId)
                .ToHashSet();

            foreach (var id in requestedIds.Where(x => !currentIds.Contains(x)))
            {
                investor.InvestorIndustries.Add(new InvestorIndustry
                {
                    InvestorId = investor.InvestorId,
                    IndustryOptionId = id
                });
            }
        }

    }
}



