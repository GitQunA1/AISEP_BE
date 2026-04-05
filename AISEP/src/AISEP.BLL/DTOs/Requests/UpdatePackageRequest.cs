using System.ComponentModel.DataAnnotations;

namespace AISEP.BLL.DTOs.Requests
{
    public class UpdatePackageRequest
    {
        [Required]
        [MaxLength(255)]
        public string PackageName { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "DurationMonths must be greater than 0.")]
        public int DurationMonths { get; set; }

        [Range(0, int.MaxValue)]
        public int MaxAiRequests { get; set; }

        [Range(0, int.MaxValue)]
        public int MaxProjectViews { get; set; }

        [Range(0, int.MaxValue)]
        public int FreeBookingCount { get; set; }
    }
}
