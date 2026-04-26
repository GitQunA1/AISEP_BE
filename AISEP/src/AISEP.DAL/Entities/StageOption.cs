namespace AISEP.DAL.Entities
{
    public class StageOption
    {
        public int Id { get; set; }
        public string Value { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Project> Projects { get; set; } = new List<Project>();
        public ICollection<Investor> Investors { get; set; } = new List<Investor>();
    }
}
