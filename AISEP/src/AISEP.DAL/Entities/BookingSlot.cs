namespace AISEP.DAL.Entities
{
    public class BookingSlot
    {
        public int BookingSlotId { get; set; }
        public int BookingId { get; set; }
        public int AdvisorAvailabilityId { get; set; }

        public Booking Booking { get; set; } = null!;
        public AdvisorAvailability AdvisorAvailability { get; set; } = null!;
    }
}
