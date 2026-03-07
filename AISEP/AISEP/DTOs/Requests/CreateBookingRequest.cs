namespace AISEP.DTOs.Requests
{
    public class CreateBookingRequest
    {
        public int AdvisorId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
