namespace AISEP.BLL.DTOs.Requests
{
    public class RejectMonthlyPayoutRequest
    {
        public string Reason { get; set; } = string.Empty;
        public string? Note { get; set; }
    }
}
