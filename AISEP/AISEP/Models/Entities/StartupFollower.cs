using System;

namespace AISEP.Models.Entities
{
    public class StartupFollower
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid StartupId { get; set; }
        public Startup Startup { get; set; } = null!;

        public DateTime FollowedAt { get; set; } = DateTime.UtcNow;
    }
}
