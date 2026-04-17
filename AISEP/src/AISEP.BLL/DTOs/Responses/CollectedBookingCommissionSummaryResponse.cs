namespace AISEP.BLL.DTOs.Responses
{
    public class CollectedBookingCommissionSummaryResponse
    {
        public decimal TotalCommissionAmount { get; set; }
        public int BookingCount { get; set; }
        public List<CollectedBookingCommissionItemResponse> Items { get; set; } = [];
    }
}
