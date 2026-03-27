namespace AISEP.BLL.DTOs.Requests
{
    public class CreateConsultingReportRequest
    {
        public int BookingId { get; set; }
        public string MeetingTitle { get; set; } = string.Empty;
        public string? Location { get; set; }
        public DateTime MeetingTime { get; set; }
        public string? MeetingPurpose { get; set; }
        public string? Content { get; set; }
        public string? DecisionsMade { get; set; }
    }
}
