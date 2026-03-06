namespace AISEP.Models.Entities
{
    public class StartupFollower
    {
        public int StartupFollowerId { get; set; }

        public int FollowerId { get; set; }
        public User User { get; set; } = null!;

        public int FollowedId { get; set; }
        public Startup Startup { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
