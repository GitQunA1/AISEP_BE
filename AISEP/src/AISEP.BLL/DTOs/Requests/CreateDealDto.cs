namespace AISEP.BLL.DTOs.Requests
{
    public class CreateDealDto
    {
        public int ProjectId { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; }
        public decimal? EquityPercentage { get; set; }
    }
}
