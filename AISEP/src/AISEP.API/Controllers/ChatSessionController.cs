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
    public class ChatSessionController : ControllerBase
    {
        private readonly IChatSessionService _chatSessionService;
        //private readonly IUserService _userService;
        private readonly IHubContext<ChatHub> _chatHubContext;

        public ChatSessionController(
            IChatSessionService chatSessionService,
            //IUserService userService,
            IHubContext<ChatHub> chatHubContext)
        {
            _chatSessionService = chatSessionService;
            //_userService = userService;
            _chatHubContext = chatHubContext;
        }

        [HttpPost("{bookingId:int}")]
        public async Task<IActionResult> OpenSession(int bookingId)
        {
            //var userId = _userService.GetUserId();
            var session = await _chatSessionService.OpenSessionAsync(bookingId);
            if (session is null)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Cannot open session. Booking does not exist or you are not a participant.",
                    "Bad request",
                    400));
            }

            return Ok(ApiResponse<object>.SuccessResponse(session, "Chat session opened."));
        }

        [HttpGet("{sessionId:int}")]
        public async Task<IActionResult> GetSession(int sessionId)
        {
           
            var session = await _chatSessionService.GetSessionAsync(sessionId);
            if (session is null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Session not found.", "Not found", 404));
            }

            return Ok(ApiResponse<object>.SuccessResponse(session, "Success"));
        }

        [HttpGet("connection-request/{connectionRequestId:int}")]
        [Authorize(Roles = "Startup,Investor")]
        public async Task<IActionResult> GetSessionByConnectionRequest(int connectionRequestId)
        {
            
            var session = await _chatSessionService.GetSessionByConnectionRequestAsync(connectionRequestId);
            if (session is null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "Session not found. Connection request may be unavailable, not accepted, or inaccessible.",
                    "Not found",
                    404));
            }

            return Ok(ApiResponse<object>.SuccessResponse(session, "Success"));
        }

        [HttpGet]
        public async Task<IActionResult> GetMySessions()
        {
            
            var sessions = await _chatSessionService.GetMySessionsAsync();
            return Ok(ApiResponse<object>.SuccessResponse(sessions, "Success"));
        }

        [HttpPatch("{sessionId:int}/close")]
        public async Task<IActionResult> CloseSession(int sessionId)
        {
            
            var closed = await _chatSessionService.CloseSessionAsync(sessionId);
            if (!closed)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Cannot close session. Session does not exist or you do not have permission.",
                    "Bad request",
                    400));
            }

            await _chatHubContext.Clients
                .Group($"chat_session_{sessionId}")
                .SendAsync(ChatHub.ClientMethodSessionClosed, new { sessionId });

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Session closed."));
        }
    }
}
