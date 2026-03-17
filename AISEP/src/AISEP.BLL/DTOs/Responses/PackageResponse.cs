namespace AISEP.BLL.DTOs.Responses
{
    public class PackageResponse
    {
        public int PackageId { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int DurationMonths { get; set; }
    }
}
