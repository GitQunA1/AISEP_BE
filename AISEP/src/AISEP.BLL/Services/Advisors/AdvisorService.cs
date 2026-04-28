using AISEP.BLL.Helpers;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Services.Storage;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AutoMapper;
using Sieve.Models;
using Sieve.Services;
using AISEP.BLL.Services.Users;
using AISEP.BLL.Services.Wallets;
using AISEP.BLL.Services.Notifications;
using AISEP.BLL.Services.FormValidationRules;
using AISEP.DAL.Enums;
using AISEP.BLL.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AISEP.BLL.Services.Advisors
{
    public class AdvisorService : IAdvisorService
    {
        private const int MaxAdvisorIndustries = 4;

        private readonly IUnitOfWork     _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper         _mapper;
        private readonly IStorageService _storage;
        private readonly IUserService _userService;
        private readonly IWalletService _walletService;
        private readonly INotificationService _notificationService;
        private readonly IDynamicFormSubmissionValidationService _dynamicFormValidationService;

        public AdvisorService(IUnitOfWork unitOfWork, ISieveProcessor sieveProcessor, IMapper mapper, IStorageService storage, IUserService userService, IWalletService walletService, INotificationService notificationService, IDynamicFormSubmissionValidationService dynamicFormValidationService)
        {
            _unitOfWork     = unitOfWork;
            _sieveProcessor = sieveProcessor;
            _mapper         = mapper;
            _storage        = storage;
            _userService = userService;
            _walletService = walletService;
            _notificationService = notificationService;
            _dynamicFormValidationService = dynamicFormValidationService;
        }

       
        public async Task<PagedResult<AdvisorResponse>> GetAllAsync(SieveModel model)
        {
            var query = _unitOfWork.Advisors.GetAllQuery();
            return await PaginationHelper.PaginateAsync(query, model, _sieveProcessor,
                a => _mapper.Map<AdvisorResponse>(a));
        }

        
        public async Task<AdvisorResponse?> GetByIdAsync(int advisorId)
        {
            var advisor = await _unitOfWork.Advisors.GetByIdAsync(advisorId);
            if (advisor is null)
                throw new KeyNotFoundException("Advisor not found.");
            return _mapper.Map<AdvisorResponse>(advisor);
        }

        
        public async Task<AdvisorResponse?> GetMyProfileAsync()
        {
            var userId = _userService.GetUserId();
            var advisor = await _unitOfWork.Advisors.GetByUserIdAsync(userId);
            if (advisor is null)
                throw new KeyNotFoundException("Advisor profile not found.");
            return _mapper.Map<AdvisorResponse>(advisor);
        }

        public async Task<AdvisorResponse?> CreateAsync(CreateAdvisorRequest dto)
        {
            
            await _dynamicFormValidationService.ValidateAsync("advisor.create", dto);

            var userId =  _userService.GetUserId();
            var existing = await _unitOfWork.Advisors.GetByUserIdAsync(userId);
            if (existing is not null)
            {
                throw new InvalidOperationException("You already have an advisor profile.");
            }

            var advisor        = _mapper.Map<Advisor>(dto);
            advisor.UserId     = userId;
            advisor.ProfileImage   = await UploadIfPresent(dto.ProfileImageFile,  "advisor-profiles");
            advisor.Certifications = await UploadIfPresent(dto.CertificationFile, "advisor-certifications");
            var industries = await ResolveIndustryOptionsAsync(dto.IndustryOptionIds);

            advisor.AdvisorIndustries = industries
                .Select(industry => new AdvisorIndustry { IndustryOptionId = industry.Id })
                .ToList();
            advisor.ApprovalStatus = ApprovalStatus.Pending;
            //advisor.CreatedAt      = DateTime.UtcNow;
            await _unitOfWork.Advisors.AddAsync(advisor);
            await _unitOfWork.SaveChangesAsync();
            await NotifyStaffAndAdminsAsync(
                "Hồ sơ cố vấn chờ duyệt",
                "Hồ sơ cố vấn mới đã được gửi và đang chờ phê duyệt.",
                advisor.AdvisorId,
                "Advisor");

            var created = await _unitOfWork.Advisors.GetByIdAsync(advisor.AdvisorId);
            return _mapper.Map<AdvisorResponse>(created!);
        }

        public async Task<AdvisorResponse?> UpdateAsync(int id, UpdateAdvisorRequest dto)
        {   
         
            await _dynamicFormValidationService.ValidateAsync("advisor.update", dto);

            var userId = _userService.GetUserId();
            var advisor = await _unitOfWork.Advisors.GetByIdAsync(id);
            if (advisor is null)
                throw new KeyNotFoundException("Advisor profile not found.");
            if (advisor.UserId != userId)
                throw new ForbiddenAccessException("You do not have permission to update this advisor.");
            if (advisor.ApprovalStatus == ApprovalStatus.Pending)
                throw new InvalidOperationException("Your advisor profile is already pending approval.");

            if (dto.Bio is not null)
                advisor.Bio = dto.Bio.Trim();

            if (dto.Expertise is not null)
                advisor.Expertise = dto.Expertise.Trim();

            if (dto.PreviousExperience is not null)
                advisor.PreviousExperience = dto.PreviousExperience.Trim();

            if (dto.LanguagesSpoken is not null)
                advisor.LanguagesSpoken = dto.LanguagesSpoken.Trim();

            if (dto.Location is not null)
                advisor.Location = dto.Location.Trim();

            if (dto.HourlyRate.HasValue)
                advisor.HourlyRate = dto.HourlyRate.Value;

            var hasIndustryUpdate = dto.IndustryOptionIds is not null;
            if (hasIndustryUpdate)
            {
                var requestedIndustries = await ResolveIndustryOptionsAsync(dto.IndustryOptionIds);
                SyncAdvisorIndustries(advisor, requestedIndustries.Select(x => x.Id));
            }

            if (dto.ProfileImageFile is not null)
                advisor.ProfileImage = await _storage.UploadFileAsync(dto.ProfileImageFile, "advisor-profiles");

            if (dto.CertificationFile is not null)
                advisor.Certifications = await _storage.UploadFileAsync(dto.CertificationFile, "advisor-certifications");

            advisor.ApprovalStatus = ApprovalStatus.Pending;
            advisor.ApprovedAt = null;
            advisor.ApprovedById = null;
            advisor.RejectedAt = null;
            advisor.RejectedById = null;
            advisor.RejectionReason = null;
            await _walletService.SyncWithAdvisorApprovalStatusAsync(advisor.AdvisorId, advisor.ApprovalStatus, createWalletIfApproved: false);

            _unitOfWork.Advisors.Update(advisor);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AdvisorResponse>(advisor);
        }

        public async Task<bool> DeleteAsync(int advisorId)
        {
            var advisor = await _unitOfWork.Advisors.GetByIdAsync(advisorId);
            if (advisor is null) return false;

            await _unitOfWork.Advisors.DeleteAsync(advisorId);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task ApproveAdvisorAsync(int advisorId)
        {
            var userId = _userService.GetUserId();
            var advisor = await _unitOfWork.Advisors.GetByIdAsync(advisorId);
            if (advisor is null)
                throw new KeyNotFoundException("Advisor not found.");
            if (advisor.ApprovalStatus == ApprovalStatus.Approved)
                throw new InvalidOperationException("Advisor is already approved.");
            if (advisor.ApprovalStatus != ApprovalStatus.Pending)
                throw new InvalidOperationException($"Only Pending advisors can be approved. Current status: {advisor.ApprovalStatus}.");

            advisor.ApprovalStatus = ApprovalStatus.Approved;
            advisor.ApprovedAt     = DateTime.UtcNow;
            advisor.ApprovedById   = userId;
            await _walletService.SyncWithAdvisorApprovalStatusAsync(advisor.AdvisorId, advisor.ApprovalStatus, createWalletIfApproved: true);

            _unitOfWork.Advisors.Update(advisor);
            await _unitOfWork.SaveChangesAsync();
            await _notificationService.SendNotificationAsync(
                advisor.UserId,
                "Hồ sơ cố vấn đã được duyệt",
                "Hồ sơ cố vấn của bạn đã được duyệt và ví hiện đang hoạt động.",
                NotificationType.General,
                advisor.AdvisorId,
                "Advisor");
        }

        public async Task RejectAdvisorAsync(int advisorId, string rejectionReason)
        {
            var userId = _userService.GetUserId();
            var advisor = await _unitOfWork.Advisors.GetByIdAsync(advisorId);
            if (advisor is null)
                throw new KeyNotFoundException("Advisor not found.");
            if (advisor.ApprovalStatus == ApprovalStatus.Rejected)
                throw new InvalidOperationException("Advisor is already rejected.");
            if (advisor.ApprovalStatus != ApprovalStatus.Pending)
                throw new InvalidOperationException($"Only Pending advisors can be rejected. Current status: {advisor.ApprovalStatus}.");

            advisor.ApprovalStatus  = ApprovalStatus.Rejected;
            advisor.RejectedAt      = DateTime.UtcNow;
            advisor.RejectedById    = userId;
            advisor.RejectionReason = rejectionReason;
            await _walletService.SyncWithAdvisorApprovalStatusAsync(advisor.AdvisorId, advisor.ApprovalStatus, createWalletIfApproved: false);

            _unitOfWork.Advisors.Update(advisor);
            await _unitOfWork.SaveChangesAsync();
            await _notificationService.SendNotificationAsync(
                advisor.UserId,
                "Hồ sơ cố vấn bị từ chối",
                $"Hồ sơ cố vấn của bạn đã bị từ chối. Lý do: {rejectionReason}",
                NotificationType.General,
                advisor.AdvisorId,
                "Advisor");
        }

        

        private async Task<string?> UploadIfPresent(IFormFile? file, string folder)
            => file is not null ? await _storage.UploadFileAsync(file, folder) : null;

        // Kiểm tra danh sách ngành của advisor có tồn tại và đang active hay không.
        private async Task<List<IndustryOption>> ResolveIndustryOptionsAsync(IEnumerable<int>? optionIds)
        {
            // distinct để loại bỏ trùng lặp, tránh lỗi khi advisor chọn cùng một ngành nhiều lần. Nếu optionIds là null thì trả về list rỗng.
            var ids = optionIds?
                .Distinct()
                .ToList() ?? [];

            if (ids.Count == 0)
            {
                throw new InvalidOperationException("At least one industry is required.");
            }
            if (ids.Count > MaxAdvisorIndustries)
            {
                throw new InvalidOperationException($"Advisor can select up to {MaxAdvisorIndustries} industries.");
            }

            var options = await _unitOfWork.IndustryOptions.GetByIdsAsync(ids);
            if (options.Count != ids.Count || options.Any(x => !x.IsActive))
            {
                throw new InvalidOperationException("One or more selected industries are invalid or inactive.");
            }

            return options;
        }

        // Đồng bộ bảng advisor_industries theo danh sách ngành mới nhất từ request.
        private static void SyncAdvisorIndustries(Advisor advisor, IEnumerable<int> industryOptionIds)
        {
            var requestedIds = industryOptionIds.ToHashSet();
            if (requestedIds.Count == 0)
            {
                throw new InvalidOperationException("At least one industry is required.");
            }
            if (requestedIds.Count > MaxAdvisorIndustries)
            {
                throw new InvalidOperationException($"Advisor can select up to {MaxAdvisorIndustries} industries.");
            }

            var toRemove = advisor.AdvisorIndustries
                .Where(x => !requestedIds.Contains(x.IndustryOptionId))
                .ToList();

            foreach (var item in toRemove)
            {
                advisor.AdvisorIndustries.Remove(item);
            }

            var currentIds = advisor.AdvisorIndustries
                .Select(x => x.IndustryOptionId)
                .ToHashSet();

            foreach (var id in requestedIds.Where(x => !currentIds.Contains(x)))
            {
                advisor.AdvisorIndustries.Add(new AdvisorIndustry
                {
                    AdvisorId = advisor.AdvisorId,
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

