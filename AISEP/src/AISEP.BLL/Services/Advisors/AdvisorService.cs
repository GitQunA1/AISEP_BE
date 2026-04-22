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
using AISEP.DAL.Enums;
using AISEP.BLL.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AISEP.BLL.Services.Advisors
{
    public class AdvisorService : IAdvisorService
    {
        private readonly IUnitOfWork     _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper         _mapper;
        private readonly IStorageService _storage;
        private readonly IUserService _userService;
        private readonly IWalletService _walletService;
        private readonly INotificationService _notificationService;

        public AdvisorService(IUnitOfWork unitOfWork, ISieveProcessor sieveProcessor, IMapper mapper, IStorageService storage, IUserService userService, IWalletService walletService, INotificationService notificationService)
        {
            _unitOfWork     = unitOfWork;
            _sieveProcessor = sieveProcessor;
            _mapper         = mapper;
            _storage        = storage;
            _userService = userService;
            _walletService = walletService;
            _notificationService = notificationService;
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
            var industries = ResolveRequestedIndustries(dto.Industries);
            if (industries.Count == 0)
                throw new InvalidOperationException("At least one industry is required.");

            advisor.AdvisorIndustries = industries
                .Select(industry => new AdvisorIndustry { Industry = industry })
                .ToList();
            advisor.ApprovalStatus = ApprovalStatus.Pending;
            //advisor.CreatedAt      = DateTime.UtcNow;
            await _unitOfWork.Advisors.AddAsync(advisor);
            await _unitOfWork.SaveChangesAsync();
            await NotifyStaffAndAdminsAsync(
                "Hồ sơ cố vấn chờ duyệt",
                "Hồ sơ cố vấn mới đã được gửi và đang chờ phê duyệt.");

            var created = await _unitOfWork.Advisors.GetByIdAsync(advisor.AdvisorId);
            return _mapper.Map<AdvisorResponse>(created!);
        }

        public async Task<AdvisorResponse?> UpdateAsync(int id, UpdateAdvisorRequest dto)
        {   
            var userId = _userService.GetUserId();
            var advisor = await _unitOfWork.Advisors.GetByIdAsync(id);
            if (advisor is null)
                throw new KeyNotFoundException("Advisor profile not found.");
            if (advisor.UserId != userId)
                throw new ForbiddenAccessException("You do not have permission to update this advisor.");

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

            var hasIndustryUpdate = dto.Industries is not null;
            if (hasIndustryUpdate)
            {
                var requestedIndustries = ResolveRequestedIndustries(dto.Industries).ToHashSet();
                if (requestedIndustries.Count == 0)
                    throw new InvalidOperationException("At least one industry is required.");

                var currentIndustries = advisor.AdvisorIndustries
                    .Select(ai => ai.Industry)
                    .ToHashSet();

                var toRemove = advisor.AdvisorIndustries
                    .Where(ai => !requestedIndustries.Contains(ai.Industry))
                    .ToList();

                foreach (var item in toRemove)
                {
                    advisor.AdvisorIndustries.Remove(item);
                }

                var toAdd = requestedIndustries
                    .Where(industry => !currentIndustries.Contains(industry));

                foreach (var industry in toAdd)
                {
                    advisor.AdvisorIndustries.Add(new AdvisorIndustry
                    {
                        AdvisorId = advisor.AdvisorId,
                        Industry = industry
                    });
                }
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

        private static List<Industry> ResolveRequestedIndustries(List<Industry>? industries)
        {
            var merged = new List<Industry>();

            if (industries is not null && industries.Count > 0)
            {
                merged.AddRange(industries);
            }

            return merged.Distinct().ToList();
        }

        private async Task NotifyStaffAndAdminsAsync(string title, string message)
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
                    NotificationType.System);
            }
        }
    }
}

