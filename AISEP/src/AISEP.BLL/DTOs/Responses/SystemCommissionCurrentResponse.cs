namespace AISEP.BLL.DTOs.Responses
{
    public class SystemCommissionCurrentResponse
    {
        public int? ConfigId { get; set; }
        public decimal Percent { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsConfigured { get; set; }
    }
}
