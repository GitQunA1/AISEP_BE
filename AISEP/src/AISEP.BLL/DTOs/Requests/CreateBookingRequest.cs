namespace AISEP.BLL.DTOs.Requests
{
    public class CreateBookingRequest
    {
        public int AdvisorId { get; set; }
        public List<int> AdvisorAvailabilitySlotIds { get; set; } = [];
        public string? Note { get; set; }
    }
}
