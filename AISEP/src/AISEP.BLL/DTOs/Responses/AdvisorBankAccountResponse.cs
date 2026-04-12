namespace AISEP.BLL.DTOs.Responses
{
    public class AdvisorBankAccountResponse
    {
        public int AdvisorBankAccountId { get; set; }
        public int AdvisorId { get; set; }
        public string AdvisorName { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountHolderName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
