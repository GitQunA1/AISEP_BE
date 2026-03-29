namespace AISEP.BLL.DTOs.Requests
{
    public class CreateBookingRequest
    {
        public int AdvisorId { get; set; }
        public int ProjectId { get; set; }
        public int? SourceBookingId { get; set; }
        public List<int> AdvisorAvailabilitySlotIds { get; set; } = [];
        public string? Note { get; set; }
    }
}
