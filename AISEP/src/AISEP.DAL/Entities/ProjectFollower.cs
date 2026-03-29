namespace AISEP.DAL.Entities
{
    public class ProjectFollower
    {
        public int ProjectFollowerId { get; set; }

        public int FollowerId { get; set; }
        public User User { get; set; } = null!;

        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
