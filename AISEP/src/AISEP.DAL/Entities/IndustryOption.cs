namespace AISEP.DAL.Entities
{
    public class IndustryOption
    {
        public int Id { get; set; }
        public string Value { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<StartupIndustry> StartupIndustries { get; set; } = new List<StartupIndustry>();
        public ICollection<Project> Projects { get; set; } = new List<Project>();
        public ICollection<InvestorIndustry> InvestorIndustries { get; set; } = new List<InvestorIndustry>();
        public ICollection<AdvisorIndustry> AdvisorIndustries { get; set; } = new List<AdvisorIndustry>();
    }
}
