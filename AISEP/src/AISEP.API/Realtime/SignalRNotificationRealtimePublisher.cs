using AISEP.API.Hubs;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Services.Notifications;
using Microsoft.AspNetCore.SignalR;

namespace AISEP.API.Realtime
{
    public class SignalRNotificationRealtimePublisher : INotificationRealtimePublisher
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRNotificationRealtimePublisher(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task PublishToUserAsync(int userId, NotificationDto notification)
        {
            await _hubContext
                .Clients
                .User(userId.ToString())
                .SendAsync(NotificationHub.ClientMethodReceiveNotification, notification);
        }
    }
}
