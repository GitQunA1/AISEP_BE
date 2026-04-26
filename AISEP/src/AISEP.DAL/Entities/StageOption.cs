using Sieve.Attributes;

namespace AISEP.DAL.Entities
{
    public class StageOption
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

        public ICollection<Project> Projects { get; set; } = new List<Project>();
        public ICollection<Investor> Investors { get; set; } = new List<Investor>();
    }
}
