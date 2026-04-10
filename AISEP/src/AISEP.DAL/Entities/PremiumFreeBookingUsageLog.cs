namespace AISEP.DAL.Entities
{
    public class PremiumFreeBookingUsageLog
    {
        public int PremiumFreeBookingUsageLogId { get; set; }
        public int UserId { get; set; }
        public int SubscriptionId { get; set; }
        public int BookingId { get; set; }
        public DateTime UsedAt { get; set; }
        public decimal BookingDurationHours { get; set; }
        public string? Note { get; set; }

        public User User { get; set; } = null!;
        public Subscription Subscription { get; set; } = null!;
        public Booking Booking { get; set; } = null!;
    }
}
