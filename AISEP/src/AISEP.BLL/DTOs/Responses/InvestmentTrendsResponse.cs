namespace AISEP.BLL.DTOs.Responses
{
    public class InvestmentTrendsResponse
    {
        public List<MonthlyInvestmentAmountResponse> MonthlyAmounts { get; set; } = [];
        public InvestmentTypeBreakdownResponse TypeBreakdown { get; set; } = new();
        public List<InvestmentTopProjectResponse> TopProjects { get; set; } = [];
    }

    public class MonthlyInvestmentAmountResponse
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Amount { get; set; }
    }

    public class InvestmentTypeBreakdownResponse
    {
        public decimal EquityPercent { get; set; }
        public decimal CustomTermsPercent { get; set; }
    }

    public class InvestmentTopProjectResponse
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public decimal TotalInvestedAmount { get; set; }
    }
}
