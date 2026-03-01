using System.ComponentModel.DataAnnotations.Schema;

namespace AISEP.Models.Entities
{
    public class Review
    {
        public int ReviewId { get; set; }

        public int AdvisorId { get; set; }

        public int ReviewerId { get; set; }

        public int Rating { get; set; }

        public string? ReviewContent { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Advisor Advisor { get; set; } = null!;

        public User Reviewer { get; set; } = null!;
    }
}
