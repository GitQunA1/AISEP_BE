using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AISEP.Models.Enums;
using Microsoft.AspNetCore.Identity;

namespace AISEP.Models.Entities
{
    public class User : IdentityUser<int>
    {
        public UserRole Role { get; set; }

        public UserStatus Status { get; set; }

        public bool IsVerified { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Startup? Startup { get; set; }
        public Investor? Investor { get; set; }
        public Advisor? Advisor { get; set; }
        public ICollection<Booking> CustomerBookings { get; set; } = new List<Booking>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public ICollection<ActionLog> ActionLogs { get; set; } = new List<ActionLog>();
        public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        // Followed startups (many-to-many)
        public ICollection<StartupFollower> FollowedStartups { get; set; } = new List<StartupFollower>();

        // Reports
        public ICollection<UserReport> ReportsMade { get; set; } = new List<UserReport>();
        public ICollection<UserReport> ReportsReceived { get; set; } = new List<UserReport>();
    }
}
