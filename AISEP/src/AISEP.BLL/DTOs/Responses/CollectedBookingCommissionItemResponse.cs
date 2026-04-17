namespace AISEP.BLL.DTOs.Responses
{
    public class CollectedBookingCommissionItemResponse
    {
        public int BookingId { get; set; }
        public decimal CommissionPercent { get; set; }
        public decimal CommissionAmount { get; set; }
    }
}
