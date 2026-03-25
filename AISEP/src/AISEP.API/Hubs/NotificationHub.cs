using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AISEP.API.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public const string ClientMethodReceiveNotification = "notification_received";
    }
}
