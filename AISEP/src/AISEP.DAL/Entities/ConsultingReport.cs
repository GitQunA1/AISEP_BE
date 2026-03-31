using AISEP.DAL.Enums;

namespace AISEP.DAL.Entities
{
    public class ConsultingReport
    {
        public int ConsultingReportId { get; set; }

        public int BookingId { get; set; }

        public string MeetingTitle { get; set; } = string.Empty;

        public string? Location { get; set; }

        public DateTime MeetingTime { get; set; }

        public string? MeetingPurpose { get; set; }

        public string? Content { get; set; }

        public string? DecisionsMade { get; set; }

        public ConsultingReportStatus Status { get; set; } = ConsultingReportStatus.Submitted;

        public int RevisionCount { get; set; } = 0;

        public string? RevisionRequestReason { get; set; }

        public DateTime LastSubmittedAt { get; set; }

        public DateTime? StartupReviewDueAt { get; set; }

        public DateTime? AdvisorRevisionDueAt { get; set; }

        public DateTime? StartupReviewedAt { get; set; }

        public bool IsPayoutProcessed { get; set; }

        public decimal? AdvisorPayoutAmount { get; set; }

        public DateTime? PayoutProcessedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Booking Booking { get; set; } = null!;
    }
}
