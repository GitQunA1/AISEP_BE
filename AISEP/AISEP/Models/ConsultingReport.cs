namespace AISEP.Models
{
    public class ConsultingReport
    {
        public Guid Id { get; set; }

        public Guid BookingId { get; set; }

        public string MeetingTitle { get; set; } = string.Empty;

        public string? Location { get; set; }

        public DateTime MeetingTime { get; set; }

        public string? MeetingPurpose { get; set; }

        public string? Content { get; set; }

        public string? DecisionsMade { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Booking Booking { get; set; } = null!;
    }
}
