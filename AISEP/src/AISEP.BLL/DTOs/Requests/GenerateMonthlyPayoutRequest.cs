namespace AISEP.BLL.DTOs.Requests
{
    public class GenerateMonthlyPayoutRequest
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? AdvisorId { get; set; }
    }
}
