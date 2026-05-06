namespace AISEP.BLL.DTOs.Responses
{
    public class PlatformRevenueStatisticsResponse
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal PeriodRevenue { get; set; }
        public decimal MonthRevenue { get; set; }
        public decimal YearRevenue { get; set; }
        public PlatformRevenueRoleBreakdownResponse RoleBreakdown { get; set; } = new();
        public BestsellerPackageResponse? BestsellerPackage { get; set; }
    }

    public class PlatformRevenueRoleBreakdownResponse
    {
        public decimal StartupRevenue { get; set; }
        public decimal InvestorRevenue { get; set; }
        public decimal StartupPercent { get; set; }
        public decimal InvestorPercent { get; set; }
    }

    public class BestsellerPackageResponse
    {
        public int PackageId { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public string TargetRole { get; set; } = string.Empty;
        public int PurchaseCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
