using System.ComponentModel.DataAnnotations;

namespace AISEP.BLL.DTOs.Requests
{
    public class RequestReportRevisionRequest
    {
        [Required]
        [MinLength(3)]
        [MaxLength(2000)]
        public string Reason { get; set; } = string.Empty;
    }
}
