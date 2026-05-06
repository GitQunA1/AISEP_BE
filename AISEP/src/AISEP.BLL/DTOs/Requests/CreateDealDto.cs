using AISEP.DAL.Enums;
using Microsoft.AspNetCore.Http;

namespace AISEP.BLL.DTOs.Requests
{
    public class CreateDealDto
    {
        public int ProjectId { get; set; }
        public decimal InvestedAmount { get; set; }
        public InvestmentType Type { get; set; }
        public decimal? EquityPercentage { get; set; }
        public string? ExchangeTerms { get; set; }
        public IFormFile EvidenceFile { get; set; } = null!;
    }
}
