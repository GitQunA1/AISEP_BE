using System.ComponentModel.DataAnnotations;

namespace AISEP.BLL.DTOs.Requests
{
    public class UpdatePackageRequest
    {
        [Required(ErrorMessage = "PackageName là bắt buộc.")]
        [MaxLength(255, ErrorMessage = "PackageName không được vượt quá 255 ký tự.")]
        public string PackageName { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price phải lớn hơn 0.")]
        public decimal Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "DurationMonths phải lớn hơn 0.")]
        public int DurationMonths { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "MaxAiRequests phải lớn hơn hoặc bằng 0.")]
        public int MaxAiRequests { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "MaxProjectViews phải lớn hơn hoặc bằng 0.")]
        public int MaxProjectViews { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "FreeBookingCount phải lớn hơn hoặc bằng 0.")]
        public int FreeBookingCount { get; set; }
    }
}
