namespace AISEP.BLL.DTOs.Responses
{
    public class DealContractStatusResponse
    {
        public int DealId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal? EquityPercentage { get; set; }
        public string? ContractPdfUrl { get; set; }
        public DateTime? ContractSignedAt { get; set; }
        public int? ContractSignedByUserId { get; set; }
        public bool IsContractSigned { get; set; }
    }
}
