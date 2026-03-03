using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AISEP.Models.Enums;

namespace AISEP.Models.Entities
{
    public class Startup
    {
        public int StartupId { get; set; }
        public int UserId { get; set; }
        public string? CompanyName { get; set; }
        public string? LogoUrl { get; set; }
        public string? Founder { get; set; }
        public string? ContactInfo { get; set; }
        public string? CountryCity { get; set; }
        public string? Website { get; set; }
        public string? Industry { get; set; }
        public DevelopmentStage? DevelopmentStage { get; set; }
        public string? ProblemStatement { get; set; }
        public string? SolutionDescription { get; set; }
        public string? TargetCustomers { get; set; }
        public string? UniqueValueProposition { get; set; }
        public decimal? MarketSize { get; set; }
        public string? BusinessModel { get; set; }
        public decimal? Revenue { get; set; }
        public string? Competitors { get; set; }
        public string? TeamMembers { get; set; }
        public string? KeySkills { get; set; }
        public string? TeamExperience { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User User { get; set; } = null!;

        public ICollection<Project> Projects { get; set; } = new List<Project>();
        public ICollection<ConnectionRequest> ConnectionRequests { get; set; } = new List<ConnectionRequest>();

        // Followers (many-to-many)
        public ICollection<StartupFollower> Followers { get; set; } = new List<StartupFollower>();
    }
}
