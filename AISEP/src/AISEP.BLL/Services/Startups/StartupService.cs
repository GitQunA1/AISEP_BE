using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Exceptions;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Storage;
using AISEP.BLL.Services.Users;
using AISEP.BLL.Services.Notifications;
using AISEP.BLL.Services.FormValidationRules;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.BLL.Services.Startups
{
    public class StartupService : IStartupService
    {
        private readonly IUnitOfWork     _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper         _mapper;
        private readonly IUserService    _userService;
        private readonly IStorageService _storage;
        private readonly INotificationService _notificationService;
        private readonly IDynamicFormSubmissionValidationService _dynamicFormValidationService;

        public StartupService(IUnitOfWork unitOfWork, ISieveProcessor sieveProcessor, IMapper mapper, IUserService userService, IStorageService storage, INotificationService notificationService, IDynamicFormSubmissionValidationService dynamicFormValidationService)
        {
            _unitOfWork     = unitOfWork;
            _sieveProcessor = sieveProcessor;
            _mapper         = mapper;
            _userService    = userService;
            _storage        = storage;
            _notificationService = notificationService;
            _dynamicFormValidationService = dynamicFormValidationService;
        }

        public async Task<PagedResult<StartupResponse>> SearchStartupsAsync(SieveModel model, string? industry = null, string? stage = null)
        {
            DevelopmentStage? parsedStage = Enum.TryParse<DevelopmentStage>(stage, ignoreCase: true, out var stageResult)
                ? stageResult : null;
            var query = _unitOfWork.Startups.SearchStartupsQuery(industry, parsedStage);
            return await PaginationHelper.PaginateAsync(query, model, _sieveProcessor, s => _mapper.Map<StartupResponse>(s));
        }

        public async Task<StartupResponse?> GetStartupByIdAsync(int id)
        {
            var startup = await _unitOfWork.Startups.GetByIdAsync(id);
            if (startup is null)
                throw new KeyNotFoundException("Startup not found.");
            return _mapper.Map<StartupResponse>(startup);
        }

        public async Task<StartupResponse?> GetMyProfileAsync()
        {
            var userId = _userService.GetUserId();
            var startup = await _unitOfWork.Startups.GetByUserIdAsync(userId);
            if (startup is null)
                throw new KeyNotFoundException("Startup profile not found.");
            return _mapper.Map<StartupResponse>(startup);
        }

        public async Task<PagedResult<StartupResponse>> GetAllStartupsAsync(SieveModel model)
        {
            var query = _unitOfWork.Startups.GetStartupQuery();
            return await PaginationHelper.PaginateAsync(query, model, _sieveProcessor, s => _mapper.Map<StartupResponse>(s));
        }

        public async Task<PagedResult<StartupResponse>> GetStartupsByStatusAsync(SieveModel model, string? status = null)
        {
            ApprovalStatus? parsedStatus = Enum.TryParse<ApprovalStatus>(status, ignoreCase: true, out var statusResult)
                ? statusResult : null;
            var query = _unitOfWork.Startups.GetByStatusQuery(parsedStatus);
            return await PaginationHelper.PaginateAsync(query, model, _sieveProcessor, s => _mapper.Map<StartupResponse>(s));
        }

        public async Task<StartupResponse> CreateStartupAsync(CreateStartupRequest dto)
        { 
            
            await _dynamicFormValidationService.ValidateAsync("startup.create", dto);

            var userId = _userService.GetUserId();
            var existing = await _unitOfWork.Startups.GetByUserIdAsync(userId);
            if (existing is not null)
                throw new InvalidOperationException("Startup profile already exists for this account.");

            var logoUrl = await UploadIfPresent(dto.LogoFile, "startup-logos");
            var businessLicenseUrl = await UploadIfPresent(dto.BusinessLicenseFile, "startup-licenses");
            var industryOptions = await ResolveIndustryOptionsAsync(dto.IndustryOptionIds);

            var startup = new Startup
            {
                UserId             = userId,
                CompanyName        = dto.CompanyName,
                Founder            = dto.Founder,
                Email              = dto.Email,
                PhoneNumber        = dto.PhoneNumber,
                CountryCity        = dto.CountryCity,
                Website            = dto.Website,
                LogoUrl            = logoUrl,
                BusinessLicenseUrl = businessLicenseUrl,
                ApprovalStatus     = ApprovalStatus.Pending,
                CreatedAt          = DateTime.UtcNow
               
            };

            startup.StartupIndustries = industryOptions
                .Select(option => new StartupIndustry
                {
                    IndustryOptionId = option.Id
                })
                .ToList();

            await _unitOfWork.Startups.AddAsync(startup);
            await _unitOfWork.SaveChangesAsync();
            await NotifyStaffAndAdminsAsync(
                "Hồ sơ startup chờ duyệt",
                "Có hồ sơ startup mới đã được gửi và đang chờ phê duyệt.",
                startup.StartupId,
                "Startup");

            return _mapper.Map<StartupResponse>(startup);
        }

        public async Task<StartupResponse> UpdateStartupAsync(int id, UpdateStartupRequest dto)
        {
           
            await _dynamicFormValidationService.ValidateAsync("startup.update", dto);

            var userId = _userService.GetUserId(); 
            var startup = await _unitOfWork.Startups.GetByIdAsync(id);
            if (startup is null)
                throw new KeyNotFoundException("Startup profile not found.");

            if (startup.UserId != userId)
                throw new ForbiddenAccessException("You do not have permission to update this startup.");

            if (dto.CompanyName is not null)
                startup.CompanyName = dto.CompanyName.Trim();

            if (dto.Founder is not null)
                startup.Founder = dto.Founder.Trim();

            if (dto.Email is not null)
                startup.Email = dto.Email.Trim();

            if (dto.PhoneNumber is not null)
                startup.PhoneNumber = dto.PhoneNumber.Trim();

            if (dto.CountryCity is not null)
                startup.CountryCity = dto.CountryCity.Trim();

            if (dto.Website is not null)
                startup.Website = dto.Website.Trim();

            if (dto.IndustryOptionIds is not null)
            {
                var industryOptions = await ResolveIndustryOptionsAsync(dto.IndustryOptionIds);
                SyncStartupIndustries(startup, industryOptions.Select(x => x.Id));
            }

            if (dto.LogoFile is not null)
                startup.LogoUrl = await _storage.UploadFileAsync(dto.LogoFile, "startup-logos");

            if (dto.BusinessLicenseFile is not null)
                startup.BusinessLicenseUrl = await _storage.UploadFileAsync(dto.BusinessLicenseFile, "startup-licenses");

            startup.ApprovalStatus = ApprovalStatus.Pending;
            startup.ApprovedAt = null;
            startup.ApprovedById = null;
            startup.RejectedAt = null;
            startup.RejectedById = null;
            startup.RejectionReason = null;

            _unitOfWork.Startups.Update(startup);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<StartupResponse>(startup);
        }

        public async Task ApproveStartupAsync(int startupId)
        {
            var userId = _userService.GetUserId();
            var startup = await _unitOfWork.Startups.GetByIdAsync(startupId);
            if (startup is null)
                throw new KeyNotFoundException("Startup not found.");
            if (startup.ApprovalStatus == ApprovalStatus.Approved)
                throw new InvalidOperationException("Startup is already approved.");
            if (startup.ApprovalStatus != ApprovalStatus.Pending)
                throw new InvalidOperationException($"Only Pending startups can be approved. Current status: {startup.ApprovalStatus}.");

            startup.ApprovalStatus = ApprovalStatus.Approved;
            startup.ApprovedAt     = DateTime.UtcNow;
            startup.ApprovedById   = userId; 

            _unitOfWork.Startups.Update(startup);
            await _unitOfWork.SaveChangesAsync();
            await _notificationService.SendNotificationAsync(
                startup.UserId,
                "Hồ sơ startup đã được duyệt",
                "Hồ sơ startup của bạn đã được duyệt.",
                NotificationType.General,
                startup.StartupId,
                "Startup");
        }

        public async Task RejectStartupAsync(int startupId, RejectStartupRequest dto)
        {
            var userId = _userService.GetUserId();
            var startup = await _unitOfWork.Startups.GetByIdAsync(startupId);
            if (startup is null)
                throw new KeyNotFoundException("Startup not found.");
            if (startup.ApprovalStatus == ApprovalStatus.Rejected)
                throw new InvalidOperationException("Startup is already rejected.");
            if (startup.ApprovalStatus != ApprovalStatus.Pending)
                throw new InvalidOperationException($"Only Pending startups can be rejected. Current status: {startup.ApprovalStatus}.");

            startup.ApprovalStatus  = ApprovalStatus.Rejected;
            startup.RejectedAt      = DateTime.UtcNow;
            startup.RejectionReason = dto.Reason?.Trim();
            startup.RejectedById    = userId; 

            _unitOfWork.Startups.Update(startup);
            await _unitOfWork.SaveChangesAsync();
            await _notificationService.SendNotificationAsync(
                startup.UserId,
                "Hồ sơ startup bị từ chối",
                $"Hồ sơ startup của bạn đã bị từ chối. Lý do: {startup.RejectionReason}",
                NotificationType.General,
                startup.StartupId,
                "Startup");
        }

        private async Task<string?> UploadIfPresent(IFormFile? file, string folder)
            => file is not null ? await _storage.UploadFileAsync(file, folder) : null;

        // Chuyển danh sách id ngành từ request thành danh sách option hợp lệ đang active.
        private async Task<List<IndustryOption>> ResolveIndustryOptionsAsync(IEnumerable<int>? optionIds)
        {
            var ids = optionIds?
                .Distinct()
                .ToList() ?? [];

            if (ids.Count == 0)
            {
                throw new InvalidOperationException("At least one industry is required.");
            }

            var options = await _unitOfWork.IndustryOptions.GetByIdsAsync(ids);
            if (options.Count != ids.Count || options.Any(x => !x.IsActive))
            {
                throw new InvalidOperationException("One or more selected industries are invalid or inactive.");
            }

            return options;
        }

        // Đồng bộ bảng nối startup_industries theo danh sách ngành mới nhất từ request.
        private static void SyncStartupIndustries(Startup startup, IEnumerable<int> industryOptionIds)
        {
            var requestedIds = industryOptionIds.ToHashSet();
            if (requestedIds.Count == 0)
            {
                throw new InvalidOperationException("At least one industry is required.");
            }

            var toRemove = startup.StartupIndustries
                .Where(x => !requestedIds.Contains(x.IndustryOptionId))
                .ToList();

            foreach (var item in toRemove)
            {
                startup.StartupIndustries.Remove(item);
            }

            var currentIds = startup.StartupIndustries
                .Select(x => x.IndustryOptionId)
                .ToHashSet();

            foreach (var id in requestedIds.Where(x => !currentIds.Contains(x)))
            {
                startup.StartupIndustries.Add(new StartupIndustry
                {
                    StartupId = startup.StartupId,
                    IndustryOptionId = id
                });
            }
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
       
    }
        }
