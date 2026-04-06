namespace AISEP.BLL.DTOs.Requests
{
    public class CreateWithdrawRequestDto
    {
        public decimal Amount { get; set; }
        public string? BankName { get; set; }
        public string? BankAccount { get; set; }
    }
}
