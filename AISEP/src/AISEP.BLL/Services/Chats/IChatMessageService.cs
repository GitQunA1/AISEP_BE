using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;

namespace AISEP.BLL.Services.Chats
{
    public interface IChatMessageService
    {
        Task<IEnumerable<ChatMessageResponse>> GetMessagesAsync(int sessionId, int userId);
        Task<ChatMessageResponse?> SendMessageAsync(int sessionId, int userId, SendMessageRequest request);
    }
}
