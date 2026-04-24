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
using AISEP.BLL.Exceptions;

namespace AISEP.BLL.Services.Investors
{
    public class InvestorService : IInvestorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;
        private readonly IDynamicFormSubmissionValidationService _dynamicFormValidationService;

        public InvestorService(IUnitOfWork unitOfWork, ISieveProcessor sieveProcessor, IMapper mapper, IUserService userService, INotificationService notificationService, IDynamicFormSubmissionValidationService dynamicFormValidationService)
        {
            _unitOfWork = unitOfWork;
            _sieveProcessor = sieveProcessor;
            _mapper = mapper;
            _userService = userService;
            _notificationService = notificationService;
            _dynamicFormValidationService = dynamicFormValidationService;
        }

        // Lấy danh sách investor có phân trang/lọc.
        public async Task<PagedResult<InvestorResponse>> GetAllAsync(SieveModel model)
        {
            var query = _unitOfWork.Investors.GetAllQuery();
            return await PaginationHelper.PaginateAsync(query, model, _sieveProcessor, i => _mapper.Map<InvestorResponse>(i));
        }

        // Lấy chi tiết investor theo id.
        public async Task<InvestorResponse?> GetByIdAsync(int investorId)
        {
            var investor = await _unitOfWork.Investors.GetByIdAsync(investorId);
            if (investor is null)
                throw new KeyNotFoundException("Investor not found.");
            return _mapper.Map<InvestorResponse>(investor);
        }

        // Lấy hồ sơ investor của user hiện tại.
        public async Task<InvestorResponse?> GetMyProfileAsync()
        {
            var userId = _userService.GetUserId();
            var investor = await _unitOfWork.Investors.GetByUserIdAsync(userId);
            if (investor is null)
                throw new KeyNotFoundException("Investor profile not found.");
            return _mapper.Map<InvestorResponse>(investor);
        }

        // Tạo hồ sơ investor mới sau khi validate theo rule động của investor.create.
        public async Task<InvestorResponse?> CreateAsync(CreateInvestorRequest dto)
        {
            // Rule field-level cho create giờ lấy động từ DB thay cho create validator cũ.
            await _dynamicFormValidationService.ValidateAsync("investor.create", dto);

            var userId = _userService.GetUserId();
            var existing = await _unitOfWork.Investors.GetByUserIdAsync(userId);
            if (existing is not null)
                throw new InvalidOperationException("Investor profile already exists for this account.");

            var investor = _mapper.Map<Investor>(dto);
            investor.UserId = userId;
            investor.ApprovalStatus = ApprovalStatus.Pending;
            await _unitOfWork.Investors.AddAsync(investor);
            await _unitOfWork.SaveChangesAsync();
            await NotifyStaffAndAdminsAsync(
                "Hồ sơ investor chờ duyệt",
                $"Có hồ sơ investor mới đã được gửi và đang chờ phê duyệt.");

            var created = await _unitOfWork.Investors.GetByIdAsync(investor.InvestorId);
            return _mapper.Map<InvestorResponse>(created!);
        }

        // Cập nhật hồ sơ investor sau khi validate theo rule động của investor.update.
        public async Task<InvestorResponse?> UpdateAsync(int id, UpdateInvestorRequest dto)
        {
            // Update dùng form key riêng để rule required/optional có thể khác create.
            await _dynamicFormValidationService.ValidateAsync("investor.update", dto);

            var userId = _userService.GetUserId();

            var investor = await _unitOfWork.Investors.GetByIdAsync(id);
            if (investor is null)
                throw new KeyNotFoundException("Investor profile not found.");
            if (investor.UserId != userId)
                throw new ForbiddenAccessException("You do not have permission to update this investor.");

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

            if (dto.FocusIndustry.HasValue)
                investor.FocusIndustry = dto.FocusIndustry.Value;

            if (dto.PreferredStage.HasValue)
                investor.PreferredStage = dto.PreferredStage.Value;

            if (dto.PreviousInvestments is not null)
                investor.PreviousInvestments = dto.PreviousInvestments.Trim();

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

        // Duyệt hồ sơ investor đang ở trạng thái Pending.
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
                "Hồ sơ investor đã được duyệt",
                "Hồ sơ investor của bạn đã được duyệt.",
                NotificationType.General,
                investor.InvestorId,
                "Investor");
        }

        // Từ chối hồ sơ investor đang ở trạng thái Pending.
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
                "Hồ sơ investor bị từ chối",
                $"Hồ sơ investor của bạn đã bị từ chối. Lý do: {rejectionReason}",
                NotificationType.General,
                investor.InvestorId,
                "Investor");
        }

        // Gửi thông báo tới Staff/Admin khi có hồ sơ investor mới chờ duyệt.
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



