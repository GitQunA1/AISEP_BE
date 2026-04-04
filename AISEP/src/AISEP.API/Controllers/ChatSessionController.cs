using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Chats;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISEP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatSessionController : ControllerBase
    {
        private readonly IChatSessionService _chatSessionService;

        public ChatSessionController(
            IChatSessionService chatSessionService)
        {
            _chatSessionService = chatSessionService;
        }

        [HttpPost("{bookingId:int}")]
        public async Task<IActionResult> OpenSession(int bookingId)
        {
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

        [HttpGet]
        public async Task<IActionResult> GetMySessions()
        {
            var sessions = await _chatSessionService.GetMySessionsAsync();
            return Ok(ApiResponse<object>.SuccessResponse(sessions, "Success"));
        }
    }
}
