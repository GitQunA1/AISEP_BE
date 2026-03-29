namespace AISEP.BLL.DTOs.Requests
{
    public class SignContractRequestDto
    {
        public decimal FinalAmount { get; set; }
        public double FinalEquityPercentage { get; set; }
        public string AdditionalTerms { get; set; } = string.Empty;
        public string SignatureBase64 { get; set; } = string.Empty;
    }
}
