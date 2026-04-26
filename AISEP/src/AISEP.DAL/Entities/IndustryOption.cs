using Sieve.Attributes;

namespace AISEP.DAL.Entities
{
    public class IndustryOption
    {
        [Sieve(CanFilter = true, CanSort = true)]
        public int Id { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public string Value { get; set; } = string.Empty;

        [Sieve(CanFilter = true, CanSort = true)]
        public bool IsActive { get; set; } = true;

        [Sieve(CanFilter = true, CanSort = true)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Sieve(CanFilter = true, CanSort = true)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<StartupIndustry> StartupIndustries { get; set; } = new List<StartupIndustry>();
        public ICollection<ProjectIndustry> ProjectIndustries { get; set; } = new List<ProjectIndustry>();
        public ICollection<InvestorIndustry> InvestorIndustries { get; set; } = new List<InvestorIndustry>();
        public ICollection<AdvisorIndustry> AdvisorIndustries { get; set; } = new List<AdvisorIndustry>();
    }
}
