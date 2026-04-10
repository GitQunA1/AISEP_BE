using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.BLL.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationRealtimePublisher _realtimePublisher;
        private readonly ILogger<NotificationService> _logger;
        private readonly ISieveProcessor _sieveProcessor;

        public NotificationService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            INotificationRealtimePublisher realtimePublisher,
            ILogger<NotificationService> logger,
            ISieveProcessor sieveProcessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _realtimePublisher = realtimePublisher;
            _logger = logger;
            _sieveProcessor = sieveProcessor;
        }

        public async Task SendNotificationAsync(int userId, string title, string message, NotificationType type, int? referenceId = null, string? referenceType = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                ReferenceId = referenceId,
                ReferenceType = referenceType,
                Title = title,
                Message = message,
                Type = type.ToString(),
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            var notificationDto = _mapper.Map<NotificationDto>(notification);
            try
            {
                await _realtimePublisher.PublishToUserAsync(userId, notificationDto);
            }
            catch (Exception ex)
            {
                // Realtime push failure must not rollback persisted notifications.
                _logger.LogWarning(ex, "Failed to push realtime notification for UserId {UserId}", userId);
            }
        }

        public async Task<PagedResult<NotificationDto>> GetUserNotificationsAsync(int userId, SieveModel model)
        {
            model ??= new SieveModel();
            model.Page ??= 1;
            model.PageSize = model.PageSize is null or <= 0
                ? 10
                : Math.Min(model.PageSize.Value, 100);

            var query = _unitOfWork.Notifications.GetByUserIdQuery(userId);

            return await PaginationHelper.PaginateAsync(
                query,
                model,
                _sieveProcessor,
                notification => _mapper.Map<NotificationDto>(notification));
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int currentUserId)
        {
            var result = await _unitOfWork.Notifications.MarkAsReadAsync(notificationId, currentUserId);
            if (!result)
            {
                return false;
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(int currentUserId)
        {
            await _unitOfWork.Notifications.MarkAllAsReadAsync(currentUserId);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId, int currentUserId)
        {
            var result = await _unitOfWork.Notifications.DeleteAsync(notificationId, currentUserId);
            if (!result)
            {
                return false;
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
