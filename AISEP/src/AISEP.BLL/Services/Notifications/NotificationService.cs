using AISEP.BLL.DTOs.Responses;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace AISEP.BLL.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationRealtimePublisher _realtimePublisher;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            INotificationRealtimePublisher realtimePublisher,
            ILogger<NotificationService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _realtimePublisher = realtimePublisher;
            _logger = logger;
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

        public async Task<List<NotificationDto>> GetUserNotificationsAsync(int userId, int pageIndex = 1, int pageSize = 10)
        {
            pageIndex = pageIndex < 1 ? 1 : pageIndex;
            pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);

            var notifications = await _unitOfWork.Notifications.GetByUserIdAsync(userId, pageIndex, pageSize);
            return notifications.Select(n => _mapper.Map<NotificationDto>(n)).ToList();
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
    }
}
