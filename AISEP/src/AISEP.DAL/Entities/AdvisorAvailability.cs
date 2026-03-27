using AISEP.DAL.Enums;

namespace AISEP.DAL.Entities
{
    public class AdvisorAvailability
    {
        public int AdvisorAvailabilityId { get; set; }
        public int AdvisorId { get; set; }
        public DateTime SlotDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public AdvisorAvailabilityStatus Status { get; set; } = AdvisorAvailabilityStatus.Available;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Advisor Advisor { get; set; } = null!;
        public ICollection<BookingSlot> BookingSlots { get; set; } = new List<BookingSlot>();
    }
}
