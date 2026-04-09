using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using AISEP.DAL.Enums;
using Sieve.Models;

namespace AISEP.BLL.Services.Notifications
{
    public interface INotificationService
    {
        Task SendNotificationAsync(int userId, string title, string message, NotificationType type, int? referenceId = null, string? referenceType = null);
        Task<PagedResult<NotificationDto>> GetUserNotificationsAsync(int userId, SieveModel model);
        Task<bool> MarkAsReadAsync(int notificationId, int currentUserId);
        Task<bool> MarkAllAsReadAsync(int currentUserId);
        Task<bool> DeleteNotificationAsync(int notificationId, int currentUserId);
    }
}
