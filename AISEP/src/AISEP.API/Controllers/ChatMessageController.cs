using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Chats;
using AISEP.BLL.Services.Users;
using AISEP.API.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatMessageController : ControllerBase
    {
        private readonly IChatMessageService _chatMessageService;
        private readonly IUserService _userService;
        private readonly IHubContext<ChatHub> _chatHubContext;

        public ChatMessageController(
            IChatMessageService chatMessageService,
            IUserService userService,
            IHubContext<ChatHub> chatHubContext)
        {
            _chatMessageService = chatMessageService;
            _userService = userService;
            _chatHubContext = chatHubContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetMessages([FromQuery] int sessionId)
        {
            var userId = _userService.GetUserId();
            var messages = await _chatMessageService.GetMessagesAsync(sessionId, userId);
            return Ok(ApiResponse<object>.SuccessResponse(messages, "Success"));
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromQuery] int sessionId, [FromBody] SendMessageRequest request)
        {
            var userId = _userService.GetUserId();
            var message = await _chatMessageService.SendMessageAsync(sessionId, userId, request);
            if (message is null)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Cannot send message. Session is closed or inaccessible.",
                    "Bad request",
                    400));
            }

            await _chatHubContext.Clients
                .Group($"chat_session_{sessionId}")
                .SendAsync(ChatHub.ClientMethodMessageReceived, message);

            return Ok(ApiResponse<object>.SuccessResponse(message, "Message sent successfully."));
        }
    }
}
