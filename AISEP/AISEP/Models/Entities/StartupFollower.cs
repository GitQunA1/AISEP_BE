using System;

namespace AISEP.Models.Entities
{
    public class StartupFollower
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int StartupId { get; set; }
        public Startup Startup { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
