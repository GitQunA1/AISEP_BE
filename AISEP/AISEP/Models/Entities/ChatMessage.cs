using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISEP.Models.Entities
{
    public class ChatMessage
    {
        public Guid Id { get; set; }

        public Guid SessionId { get; set; }

        public Guid SenderId { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; }

        // Navigation properties
        public ChatSession ChatSession { get; set; } = null!;

        public User Sender { get; set; } = null!;
    }
}
