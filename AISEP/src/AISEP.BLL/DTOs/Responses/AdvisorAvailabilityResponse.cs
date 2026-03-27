using AISEP.DAL.Enums;

namespace AISEP.BLL.DTOs.Responses
{
    public class AdvisorAvailabilityResponse
    {
        public int AdvisorAvailabilityId { get; set; }
        public int AdvisorId { get; set; }
        public DateTime SlotDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public AdvisorAvailabilityStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
