using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISEP.DAL.Entities
{
    public class Package
    {
        public int PackageId { get; set; }

        public string PackageName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public int DurationMonths { get; set; }

        public int MaxAiRequests { get; set; }

        public int MaxProjectViews { get; set; }

        public int FreeBookingCount { get; set; }

        // Navigation properties
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
