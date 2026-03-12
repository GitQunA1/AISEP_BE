using AISEP.BLL.DTOs.Responses;

namespace AISEP.BLL.Services.Chats
{
    public interface IChatSessionService
    {
        Task<ChatSessionResponse?> OpenSessionAsync(int bookingId, int userId);
        Task<ChatSessionResponse?> GetSessionAsync(int sessionId, int userId);
        Task<IEnumerable<ChatSessionResponse>> GetMySessionsAsync(int userId);
        Task<bool> CloseSessionAsync(int sessionId, int userId);
    }
}
