using System.ComponentModel.DataAnnotations.Schema;
using AISEP.Models.Enums;

namespace AISEP.Models.Entities
{
    public class Subscription
    {
        public int Id { get; set; }

        public int PackageId { get; set; }

        public int UserId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public SubscriptionStatus Status { get; set; }

        // Navigation properties
        public Package Package { get; set; } = null!;

        public User User { get; set; } = null!;
    }
}
