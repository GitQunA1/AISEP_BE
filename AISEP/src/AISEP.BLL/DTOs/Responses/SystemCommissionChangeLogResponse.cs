namespace AISEP.BLL.DTOs.Responses
{
    public class SystemCommissionChangeLogResponse
    {
        public int LogId { get; set; }
        public int? ConfigId { get; set; }
        public decimal? OldPercent { get; set; }
        public decimal NewPercent { get; set; }
        public DateTime? OldEffectiveFrom { get; set; }
        public DateTime? OldEffectiveTo { get; set; }
        public DateTime NewEffectiveFrom { get; set; }
        public DateTime? NewEffectiveTo { get; set; }
        public string? Reason { get; set; }
        public int ChangedById { get; set; }
        public string ChangedByName { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
    }
}
