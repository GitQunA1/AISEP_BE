using AISEP.BLL.DTOs.Responses;

namespace AISEP.BLL.Services.Chats
{
    public interface IChatSessionService
    {
        Task<ChatSessionResponse?> OpenSessionAsync(int bookingId);
        Task<ChatSessionResponse?> OpenSessionByConnectionRequestAsync(int connectionRequestId, int userId);
        Task<ChatSessionResponse?> GetSessionAsync(int sessionId);
        Task<ChatSessionResponse?> GetSessionByConnectionRequestAsync(int connectionRequestId);
        Task<IEnumerable<ChatSessionResponse>> GetMySessionsAsync();
        Task<bool> CloseSessionAsync(int sessionId);
    }
}
