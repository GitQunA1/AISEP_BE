namespace AISEP.DAL.Entities
{
    public class ChatSession
    {
        public int ChatSessionId { get; set; }
        public int? BookingId { get; set; }
        public int? ConnectionRequestId { get; set; }
        public bool IsOpen { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        // Navigation properties
        public Booking? Booking { get; set; }
        public ConnectionRequest? ConnectionRequest { get; set; }
        public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    }
}
