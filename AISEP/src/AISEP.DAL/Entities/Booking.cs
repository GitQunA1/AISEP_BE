using AISEP.DAL.Enums;

namespace AISEP.DAL.Entities
{
    public class Booking
    {
        public int BookingId { get; set; }
        public int AdvisorId { get; set; }
        public int CustomerId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal Price { get; set; }
        public BookingStatus Status { get; set; }

        // Navigation properties
        public Advisor Advisor { get; set; } = null!;
        public User Customer { get; set; } = null!;
        public ChatSession? ChatSession { get; set; }
        public ConsultingReport? ConsultingReport { get; set; }
        public Review? Review { get; set; }
    }
}
