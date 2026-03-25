using AISEP.BLL.DTOs.Responses;
using AISEP.DAL.Enums;

namespace AISEP.BLL.Services.Notifications
{
    public interface INotificationService
    {
        Task SendNotificationAsync(int userId, string title, string message, NotificationType type);
        Task<List<NotificationDto>> GetUserNotificationsAsync(int userId, int pageIndex = 1, int pageSize = 10);
        Task<bool> MarkAsReadAsync(int notificationId, int currentUserId);
        Task<bool> MarkAllAsReadAsync(int currentUserId);
    }
}
