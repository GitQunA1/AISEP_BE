using AISEP.Models.Enums;

namespace AISEP.Models.Entities
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

        // Navigation properties
        public User User { get; set; } = null!;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public Wallet? Wallet { get; set; }
    }
}
