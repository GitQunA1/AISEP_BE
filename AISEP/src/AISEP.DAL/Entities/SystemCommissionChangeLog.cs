namespace AISEP.DAL.Entities
{
    public class SystemCommissionChangeLog
    {
        public int SystemCommissionChangeLogId { get; set; }
        public int? SystemCommissionConfigId { get; set; }
        public decimal? OldPercent { get; set; }
        public decimal NewPercent { get; set; }
        public DateTime? OldEffectiveFrom { get; set; }
        public DateTime? OldEffectiveTo { get; set; }
        public DateTime NewEffectiveFrom { get; set; }
        public DateTime? NewEffectiveTo { get; set; }
        public string? Reason { get; set; }
        public int ChangedById { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        public SystemCommissionConfig? SystemCommissionConfig { get; set; }
        public User ChangedBy { get; set; } = null!;
    }
}
