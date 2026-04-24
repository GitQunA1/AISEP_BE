using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Services.Notifications;
using AISEP.BLL.Services.Users;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

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
                Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(),
                EffectiveFrom = now,
                EffectiveTo = null,
                IsActive = true,
                CreatedById = actorId,
                CreatedAt = now
            };

            await _unitOfWork.SystemCommissionConfigs.AddAsync(config);
            await _unitOfWork.SaveChangesAsync();

            await NotifyStaffAndAdminsAsync(
                "Đã cập nhật mức phí hệ thống",
                $"Mức phí hệ thống đã được cập nhật thành {config.Percent:0.##}%.",
                config.SystemCommissionConfigId,
                "SystemCommissionConfig");

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
            var query = _unitOfWork.SystemCommissionConfigs.GetQuery();
            model ??= new SieveModel();
            if (!model.Page.HasValue || model.Page.Value <= 0)
            {
                model.Page = 1;
            }

            if (!model.PageSize.HasValue || model.PageSize.Value <= 0)
            {
                model.PageSize = 10;
            }

            var totalCount = await _sieveProcessor
                .Apply(model, query, applyPagination: false, applySorting: false)
                .CountAsync();

            var items = await _sieveProcessor
                .Apply(model, query)
                .ToListAsync();

            var responses = new List<SystemCommissionChangeLogResponse>(items.Count);
            foreach (var x in items)
            {
                var previous = await _unitOfWork.SystemCommissionConfigs.GetQuery()
                    .Where(c => c.EffectiveFrom < x.EffectiveFrom)
                    .OrderByDescending(c => c.EffectiveFrom)
                    .Select(c => new
                    {
                        c.Percent,
                        c.EffectiveFrom,
                        c.EffectiveTo
                    })
                    .FirstOrDefaultAsync();

                responses.Add(new SystemCommissionChangeLogResponse
                {
                    LogId = x.SystemCommissionConfigId,
                    ConfigId = x.SystemCommissionConfigId,
                    OldPercent = previous?.Percent,
                    NewPercent = x.Percent,
                    OldEffectiveFrom = previous?.EffectiveFrom,
                    OldEffectiveTo = previous?.EffectiveTo,
                    NewEffectiveFrom = x.EffectiveFrom,
                    NewEffectiveTo = x.EffectiveTo,
                    Reason = x.Reason,
                    ChangedById = x.CreatedById,
                    ChangedByName = x.CreatedBy.UserName ?? $"User {x.CreatedById}",
                    ChangedAt = x.CreatedAt
                });
            }

            return new PagedResult<SystemCommissionChangeLogResponse>
            {
                Page = model.Page.Value,
                PageSize = model.PageSize.Value,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)model.PageSize.Value),
                Items = responses
            };
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
