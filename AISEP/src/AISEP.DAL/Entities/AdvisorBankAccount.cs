namespace AISEP.DAL.Entities
{
    public class AdvisorBankAccount
    {
        public int AdvisorBankAccountId { get; set; }
        public int AdvisorId { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountHolderName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Advisor Advisor { get; set; } = null!;
    }
}
