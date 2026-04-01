using System.Security.Claims;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Services.Chats;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AISEP.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public const string ClientMethodMessageReceived = "chat_message_received";
        public const string ClientMethodSessionClosed = "chat_session_closed";

        private readonly IChatMessageService _chatMessageService;
        private readonly IChatSessionService _chatSessionService;

        public ChatHub(IChatMessageService chatMessageService, IChatSessionService chatSessionService)
        {
            _chatMessageService = chatMessageService;
            _chatSessionService = chatSessionService;
        }

        public async Task JoinSession(int sessionId)
        {
            //var userId = GetUserId();
            var session = await _chatSessionService.GetSessionAsync(sessionId);
            if (session is null)
            {
                throw new HubException("You do not have access to this chat session.");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, BuildSessionGroupName(sessionId));
        }

        public async Task LeaveSession(int sessionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, BuildSessionGroupName(sessionId));
        }

        public async Task SendMessage(int sessionId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new HubException("Message content is required.");
            }

            var userId = GetUserId();
            var message = await _chatMessageService.SendMessageAsync(sessionId, userId, new SendMessageRequest
            {
                Content = content.Trim()
            });

            if (message is null)
            {
                throw new HubException("Cannot send message. Session is closed or inaccessible.");
            }

            await Clients.Group(BuildSessionGroupName(sessionId))
                .SendAsync(ClientMethodMessageReceived, message);
        }

        public async Task CloseSession(int sessionId)
        {
            //var userId = GetUserId();
            var closed = await _chatSessionService.CloseSessionAsync(sessionId);
            if (!closed)
            {
                throw new HubException("Cannot close this session.");
            }

            await Clients.Group(BuildSessionGroupName(sessionId))
                .SendAsync(ClientMethodSessionClosed, new { sessionId });
        }

        private int GetUserId()
        {
            var userIdRaw = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdRaw, out var userId))
            {
                throw new HubException("Unauthorized.");
            }

            return userId;
        }

        private static string BuildSessionGroupName(int sessionId) => $"chat_session_{sessionId}";
    }
}
