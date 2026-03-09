using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISEP.Models.Entities
{
    public class ChatMessage
    {
        public int ChatMessageId { get; set; }

        public int ChatSessionId { get; set; }

        public int SenderId { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; }

        // Navigation properties
        public ChatSession ChatSession { get; set; } = null!;

        public User Sender { get; set; } = null!;
    }
}
