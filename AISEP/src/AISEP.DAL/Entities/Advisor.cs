using AISEP.DAL.Enums;

namespace AISEP.DAL.Entities
{
    public class Advisor
    {
        public int AdvisorId { get; set; }
        public int UserId { get; set; }
        public string? Bio { get; set; }
        public string? Expertise { get; set; }
        public string? Certifications { get; set; }
        public string? PreviousExperience { get; set; }
        public decimal? Rating { get; set; }
        public string? LanguagesSpoken { get; set; }
        public string? Location { get; set; }
        public string? ProfileImage { get; set; }
        public decimal? HourlyRate { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Unverified;
        public int? ApprovedById { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public int? RejectedById { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? RejectionReason { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<AdvisorAvailability> Availabilities { get; set; } = new List<AdvisorAvailability>();
        public ICollection<AdvisorIndustry> AdvisorIndustries { get; set; } = new List<AdvisorIndustry>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<ProjectAdvisorAssignment> ProjectAdvisorAssignments { get; set; } = new List<ProjectAdvisorAssignment>();
        public ICollection<AdvisorBankAccount> BankAccounts { get; set; } = new List<AdvisorBankAccount>();
        public Wallet? Wallet { get; set; }
    }
}
