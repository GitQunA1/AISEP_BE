namespace AISEP.BLL.DTOs.Requests
{
    public class CreateAdvisorBankAccountRequest
    {
        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountHolderName { get; set; } = string.Empty;
    }
}
