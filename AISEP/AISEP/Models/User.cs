using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AISEP.Models.Enums;

namespace AISEP.Models
{
    public class User
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = string.Empty;

        
        public string PasswordHash { get; set; } = string.Empty;

        
        public UserRole Role { get; set; }

        
        public UserStatus Status { get; set; }

        public bool IsEmailVerified { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Startup? Startup { get; set; }
        public Investor? Investor { get; set; }
        public Advisor? Advisor { get; set; }
        public ICollection<Project> Projects { get; set; } = new List<Project>();
        public ICollection<Booking> CustomerBookings { get; set; } = new List<Booking>();
        public ICollection<Wallet> Wallets { get; set; } = new List<Wallet>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public ICollection<ActionLog> ActionLogs { get; set; } = new List<ActionLog>();
        public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
