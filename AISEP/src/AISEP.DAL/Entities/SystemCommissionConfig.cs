namespace AISEP.DAL.Entities
{
    public class SystemCommissionConfig
    {
        public int SystemCommissionConfigId { get; set; }
        public decimal Percent { get; set; }
        public string? Reason { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsActive { get; set; }
        public int CreatedById { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User CreatedBy { get; set; } = null!;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
