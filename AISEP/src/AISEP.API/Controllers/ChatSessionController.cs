using AISEP.BLL.Common;
using AISEP.BLL.Services.Chats;
using AISEP.BLL.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISEP.API.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatSessionController : ControllerBase
    {
        //private readonly IChatSessionService _chatSessionService;
        //private readonly IUserService        _userService;

        //public ChatSessionController(IChatSessionService chatSessionService, IUserService userService)
        //{
        //    _chatSessionService = chatSessionService;
        //    _userService        = userService;
        //}

       
        //[HttpPost("{bookingId:int}")]
        //public async Task<IActionResult> OpenSession(int bookingId)
        //{
        //    var userId  = _userService.GetUserId();
        //    var session = await _chatSessionService.OpenSessionAsync(bookingId, userId);
        //    if (session is null)
        //        return BadRequest(ApiResponse<object>.ErrorResponse(
        //            "Không thể mở session. Booking không tồn tại hoặc bạn không phải participant.", "Bad request"));

        //    return Ok(ApiResponse<object>.SuccessResponse(session, "Chat session đã được mở."));
        //}

        ///// <summary>Lấy thông tin chi tiết một chat session theo ID.</summary>
        //[HttpGet("{sessionId:int}")]
        //public async Task<IActionResult> GetSession(int sessionId)
        //{
        //    var userId  = _userService.GetUserId();
        //    var session = await _chatSessionService.GetSessionAsync(sessionId, userId);
        //    if (session is null)
        //        return NotFound(ApiResponse<object>.ErrorResponse("Session không tìm thấy.", "Not found", 404));

        //    return Ok(ApiResponse<object>.SuccessResponse(session, "Success"));
        //}

        ///// <summary>Lấy tất cả chat session của user đang đăng nhập.</summary>
        //[HttpGet]
        //public async Task<IActionResult> GetMySessions()
        //{
        //    var userId   = _userService.GetUserId();
        //    var sessions = await _chatSessionService.GetMySessionsAsync(userId);
        //    return Ok(ApiResponse<object>.SuccessResponse(sessions, "Success"));
        //}

        ///// <summary>Đóng một chat session (chỉ participant mới được đóng).</summary>
        //[HttpPatch("{sessionId:int}/close")]
        //public async Task<IActionResult> CloseSession(int sessionId)
        //{
        //    var userId = _userService.GetUserId();
        //    var closed = await _chatSessionService.CloseSessionAsync(sessionId, userId);
        //    if (!closed)
        //        return BadRequest(ApiResponse<object>.ErrorResponse(
        //            "Không thể đóng session. Session không tồn tại hoặc bạn không có quyền.", "Bad request"));

        //    return Ok(ApiResponse<object>.SuccessResponse(null!, "Session đã được đóng."));
        //}
    }
}
