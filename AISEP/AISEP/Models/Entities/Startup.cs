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
        public Industry? Industry { get; set; }
        public string? BusinessLicenseUrl { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Unverified;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User User { get; set; } = null!;
        public ICollection<Project> Projects { get; set; } = new List<Project>();
        public ICollection<StartupFollower> Followers { get; set; } = new List<StartupFollower>();
    }
}
