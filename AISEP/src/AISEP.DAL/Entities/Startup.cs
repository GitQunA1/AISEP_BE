using AISEP.DAL.Enums;

namespace AISEP.DAL.Entities
{
    public class Startup
    {
        public int StartupId { get; set; }
        public int UserId { get; set; }
        public string? CompanyName { get; set; }
        public string? LogoUrl { get; set; }
        public string? Founder { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CountryCity { get; set; }
        public string? Website { get; set; }
        public string? BusinessLicenseUrl { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Unverified;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

       
        public int? ApprovedById { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public int? RejectedById { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? RejectionReason { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public ICollection<Project> Projects { get; set; } = new List<Project>();
        public ICollection<StartupIndustry> StartupIndustries { get; set; } = new List<StartupIndustry>();
    }
}
