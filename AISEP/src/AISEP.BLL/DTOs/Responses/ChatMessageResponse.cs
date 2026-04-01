namespace AISEP.BLL.DTOs.Responses
{
    public class ChatMessageResponse
    {
        public int ChatMessageId { get; set; }
        public int ChatSessionId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}
