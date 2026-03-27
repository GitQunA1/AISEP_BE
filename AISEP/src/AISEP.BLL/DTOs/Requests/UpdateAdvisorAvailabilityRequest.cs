namespace AISEP.BLL.DTOs.Requests
{
    public class UpdateAdvisorAvailabilityRequest
    {
        public DateTime SlotDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
