using AISEP.DAL.Enums;

namespace AISEP.DAL.Entities
{
    public class Booking
    {
        public int BookingId { get; set; }
        public int AdvisorId { get; set; }
        public int? ProjectId { get; set; }
        public int CustomerId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal Price { get; set; }
        public int? SystemCommissionConfigId { get; set; }
        public decimal SystemCommissionAmount { get; set; }
        public int? OldBookingId { get; set; }
        public bool IsFreeRebookFromComplaint { get; set; }
        public bool IsPaymentWaived { get; set; }
        public bool UsedPremiumFreeQuota { get; set; }
        public bool PremiumFreeQuotaRefunded { get; set; }
        public BookingStatus Status { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Advisor Advisor { get; set; } = null!;
        public Project? Project { get; set; }
        public User Customer { get; set; } = null!;
        public SystemCommissionConfig? SystemCommissionConfig { get; set; }
        public Booking? OldBooking { get; set; }
        public ICollection<Booking> Rebookings { get; set; } = new List<Booking>();
        public ChatSession? ChatSession { get; set; }
        public ConsultingReport? ConsultingReport { get; set; }
        public Review? Review { get; set; }
        public ICollection<UserReport> UserReports { get; set; } = new List<UserReport>();
        public ICollection<BookingSlot> BookingSlots { get; set; } = new List<BookingSlot>();
    }
}
