using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Notifications;
using AISEP.BLL.Services.Users;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using Sieve.Models;
using Sieve.Services;
using Microsoft.EntityFrameworkCore;

namespace AISEP.BLL.Services.SystemCommissions
{
    public class SystemCommissionService : ISystemCommissionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly INotificationService _notificationService;

        public SystemCommissionService(
            IUnitOfWork unitOfWork,
            IUserService userService,
            ISieveProcessor sieveProcessor,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _sieveProcessor = sieveProcessor;
            _notificationService = notificationService;
        }

        public async Task<SystemCommissionCurrentResponse> GetCurrentAsync()
        {
            var current = await _unitOfWork.SystemCommissionConfigs.GetCurrentAsync(DateTime.UtcNow);
            if (current is null)
            {
                return new SystemCommissionCurrentResponse
                {
                    IsConfigured = false,
                    Percent = 0m
                };
            }

            return new SystemCommissionCurrentResponse
            {
                ConfigId = current.SystemCommissionConfigId,
                Percent = current.Percent,
                EffectiveFrom = current.EffectiveFrom,
                EffectiveTo = current.EffectiveTo,
                IsConfigured = true
            };
        }

        public async Task<SystemCommissionCurrentResponse> UpdateCurrentAsync(UpdateSystemCommissionRequest request)
        {
            var now = DateTime.UtcNow;
            var actorId = _userService.GetUserId();
            var active = await _unitOfWork.SystemCommissionConfigs.GetActiveAsync();
            var oldPercent = active?.Percent;
            var oldEffectiveFrom = active?.EffectiveFrom;
            var oldEffectiveTo = active?.EffectiveTo;

            if (active is not null && active.Percent == request.Percent)
                throw new InvalidOperationException("System commission percent is already set to this value.");

            if (active is not null)
            {
                active.IsActive = false;
                active.EffectiveTo = now;
                _unitOfWork.SystemCommissionConfigs.Update(active);
            }

            var config = new SystemCommissionConfig
            {
                Percent = request.Percent,
                EffectiveFrom = now,
                EffectiveTo = null,
                IsActive = true,
                CreatedById = actorId,
                CreatedAt = now
            };
            await _unitOfWork.SystemCommissionConfigs.AddAsync(config);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.SystemCommissionChangeLogs.AddAsync(new SystemCommissionChangeLog
            {
                SystemCommissionConfigId = config.SystemCommissionConfigId,
                OldPercent = oldPercent,
                NewPercent = config.Percent,
                OldEffectiveFrom = oldEffectiveFrom,
                OldEffectiveTo = oldEffectiveTo,
                NewEffectiveFrom = config.EffectiveFrom,
                NewEffectiveTo = config.EffectiveTo,
                Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(),
                ChangedById = actorId,
                ChangedAt = now
            });
            await _unitOfWork.SaveChangesAsync();
            await NotifyStaffAndAdminsAsync(
                "Cập nhật hoa hồng hệ thống",
                $"Mức hoa hồng hệ thống đã được cập nhật thành {config.Percent:0.##}%.");

            return new SystemCommissionCurrentResponse
            {
                ConfigId = config.SystemCommissionConfigId,
                Percent = config.Percent,
                EffectiveFrom = config.EffectiveFrom,
                EffectiveTo = config.EffectiveTo,
                IsConfigured = true
            };
        }

        public async Task<PagedResult<SystemCommissionChangeLogResponse>> GetHistoryAsync(SieveModel model)
        {
            var query = _unitOfWork.SystemCommissionChangeLogs.GetQuery();
            return await PaginationHelper.PaginateAsync(query, model, _sieveProcessor, x => new SystemCommissionChangeLogResponse
            {
                LogId = x.SystemCommissionChangeLogId,
                ConfigId = x.SystemCommissionConfigId,
                OldPercent = x.OldPercent,
                NewPercent = x.NewPercent,
                OldEffectiveFrom = x.OldEffectiveFrom,
                OldEffectiveTo = x.OldEffectiveTo,
                NewEffectiveFrom = x.NewEffectiveFrom,
                NewEffectiveTo = x.NewEffectiveTo,
                Reason = x.Reason,
                ChangedById = x.ChangedById,
                ChangedByName = x.ChangedBy.UserName ?? $"User {x.ChangedById}",
                ChangedAt = x.ChangedAt
            });
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
