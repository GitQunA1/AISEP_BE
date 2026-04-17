namespace AISEP.BLL.DTOs.Requests
{
    public class RejectPayoutRequest
    {
        public string Reason { get; set; } = string.Empty;
        public string? Note { get; set; }
    }
}

