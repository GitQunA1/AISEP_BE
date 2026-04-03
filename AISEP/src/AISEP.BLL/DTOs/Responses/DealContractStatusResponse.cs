namespace AISEP.BLL.DTOs.Responses
{
    public class DealContractStatusResponse
    {
        public int DealId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal? EquityPercentage { get; set; }
        public string? AdditionalTerms { get; set; }
        public string? ContractPdfUrl { get; set; }
        public DateTime? InvestorSignedAt { get; set; }
        public DateTime? StartupSignedAt { get; set; }
        public bool IsInvestorSigned { get; set; }
        public bool IsStartupSigned { get; set; }
        public bool IsContractSigned { get; set; }
    }
}
