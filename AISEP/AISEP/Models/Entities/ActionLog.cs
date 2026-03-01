using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISEP.Models.Entities
{
    public class ActionLog
    {
        public int ActionLogId { get; set; }

        public int UserId { get; set; }

        public string ActionType { get; set; } = string.Empty;

        public string EntityType { get; set; } = string.Empty;

        public int EntityId { get; set; }

        public string? Description { get; set; }

        public DateTime Timestamp { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
    }
}
