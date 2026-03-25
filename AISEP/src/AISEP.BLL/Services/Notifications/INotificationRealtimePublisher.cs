using AISEP.BLL.DTOs.Responses;

namespace AISEP.BLL.Services.Notifications
{
    public interface INotificationRealtimePublisher
    {
        Task PublishToUserAsync(int userId, NotificationDto notification);
    }
}
