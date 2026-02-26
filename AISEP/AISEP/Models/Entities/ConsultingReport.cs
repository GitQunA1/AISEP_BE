namespace AISEP.Models.Entities
{
    public class ConsultingReport
    {
        public int Id { get; set; }

        public int BookingId { get; set; }

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
