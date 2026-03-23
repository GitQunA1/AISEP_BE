namespace AISEP.DAL.Entities
{
    public class UnlockedProject
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int ProjectId { get; set; }

        public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;

        public Project Project { get; set; } = null!;
    }
}
