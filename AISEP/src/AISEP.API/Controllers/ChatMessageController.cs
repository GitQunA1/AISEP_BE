using AISEP.BLL.Helpers;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Services.Chats;
using AISEP.BLL.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISEP.API.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatMessageController : ControllerBase
    {
        //private readonly IChatMessageService _chatMessageService;
        //private readonly IUserService        _userService;

        //public ChatMessageController(IChatMessageService chatMessageService, IUserService userService)
        //{
        //    _chatMessageService = chatMessageService;
        //    _userService        = userService;
        //}

     
        //[HttpGet]
        //public async Task<IActionResult> GetMessages(int sessionId)
        //{
        //    var userId   = _userService.GetUserId();
        //    var messages = await _chatMessageService.GetMessagesAsync(sessionId, userId);
        //    return Ok(ApiResponse<object>.SuccessResponse(messages, "Success"));
        //}

       
        //[HttpPost]
        //public async Task<IActionResult> SendMessage(int sessionId, [FromBody] SendMessageRequest request)
        //{
        //    var userId  = _userService.GetUserId();
        //    var message = await _chatMessageService.SendMessageAsync(sessionId, userId, request);
        //    if (message is null)
        //        return BadRequest(ApiResponse<object>.ErrorResponse(
        //            "Không thể gửi tin nhắn. Session đã đóng hoặc bạn không phải participant.", "Bad request"));

        //    return Ok(ApiResponse<object>.SuccessResponse(message, "Tin nhắn đã được gửi."));
        //}
    }
}
