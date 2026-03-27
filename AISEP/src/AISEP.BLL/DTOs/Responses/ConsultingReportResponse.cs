namespace AISEP.BLL.DTOs.Responses
{
    public class ConsultingReportResponse
    {
        public int ConsultingReportId { get; set; }
        public int BookingId { get; set; }
        public string MeetingTitle { get; set; } = string.Empty;
        public string? Location { get; set; }
        public DateTime MeetingTime { get; set; }
        public string? MeetingPurpose { get; set; }
        public string? Content { get; set; }
        public string? DecisionsMade { get; set; }
        public DateTime CreatedAt { get; set; }
        public int AdvisorId { get; set; }
        public string AdvisorName { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
    }
}
