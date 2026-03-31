namespace AISEP.BLL.DTOs.Responses
{
    public class ChatMessageResponse
    {
        public int    ChatMessageId  { get; set; }
        public int    ChatSessionId  { get; set; }
        public int    SenderId       { get; set; }
        public string SenderName     { get; set; } = string.Empty;
        public string Content        { get; set; } = string.Empty;
        public DateTime SentAt       { get; set; }
    }

    public class ChatSessionResponse
    {
        public int      ChatSessionId { get; set; }
        public int?     BookingId     { get; set; }
        public int?     ConnectionRequestId { get; set; }
        public string   SessionType   { get; set; } = string.Empty;
        public bool     IsOpen        { get; set; }
        public DateTime StartTime     { get; set; }
        public DateTime? EndTime      { get; set; }

        public string AdvisorName    { get; set; } = string.Empty;
        public string CustomerName   { get; set; } = string.Empty;
        public string StartupName    { get; set; } = string.Empty;
        public string InvestorName   { get; set; } = string.Empty;

        public IEnumerable<ChatMessageResponse> Messages { get; set; } = [];
    }
}
