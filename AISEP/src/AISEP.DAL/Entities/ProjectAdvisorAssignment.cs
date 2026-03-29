namespace AISEP.DAL.Entities
{
    public class ProjectAdvisorAssignment
    {
        public int ProjectId { get; set; }
        public int AdvisorId { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public Project Project { get; set; } = null!;
        public Advisor Advisor { get; set; } = null!;
    }
}
