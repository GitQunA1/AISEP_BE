using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Notifications;
using AISEP.BLL.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;

        public NotificationsController(INotificationService notificationService, IUserService userService)
        {
            _notificationService = notificationService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications([FromQuery] SieveModel model)
        {
            var userId = _userService.GetUserId();
            var data = await _notificationService.GetUserNotificationsAsync(userId, model);
            return Ok(ApiResponse<object>.SuccessResponse(data, "Notifications retrieved successfully"));
        }

        [HttpPut("{notificationId:int}/read")]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            var userId = _userService.GetUserId();
            var result = await _notificationService.MarkAsReadAsync(notificationId, userId);
            if (!result)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Notification not found or access denied.", "Not found", 404));
            }

            return Ok(ApiResponse<object>.SuccessResponse(result, "Notification marked as read"));
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = _userService.GetUserId();
            var result = await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(ApiResponse<object>.SuccessResponse(result, "All notifications marked as read"));
        }

        [HttpDelete("{notificationId:int}")]
        public async Task<IActionResult> DeleteNotification(int notificationId)
        {
            var userId = _userService.GetUserId();
            var result = await _notificationService.DeleteNotificationAsync(notificationId, userId);
            if (!result)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Notification not found or access denied.", "Not found", 404));
            }

            return Ok(ApiResponse<object>.SuccessResponse(result, "Notification deleted successfully"));
        }
    }
}
