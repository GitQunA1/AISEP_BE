namespace AISEP.BLL.DTOs.Requests
{
    public class GenerateMonthlyPayoutRequest
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int? AdvisorId { get; set; }
    }
}
