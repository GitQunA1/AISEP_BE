using System.ComponentModel.DataAnnotations.Schema;

namespace AISEP.Models.Entities
{
    public class ChatSession
    {
        public int Id { get; set; }

        public int BookingId { get; set; }

        public bool IsOpen { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        // Navigation properties
        public Booking Booking { get; set; } = null!;

        public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    }
}
