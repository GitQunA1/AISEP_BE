namespace AISEP.BLL.DTOs.Responses
{
    public class AdvisorWalletResponse
    {
        public int WalletId { get; set; }
        public int AdvisorId { get; set; }
        public int AdvisorUserId { get; set; }
        public string AdvisorName { get; set; } = string.Empty;
        public string AdvisorEmail { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string Currency { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
