using AISEP.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISEP.Models.Entities
{
    public class Notification
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Message { get; set; } = string.Empty;

        public NotificationStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
    }
}
