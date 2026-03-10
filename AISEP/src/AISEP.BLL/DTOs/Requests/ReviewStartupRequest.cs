using AISEP.DAL.Enums;
using System.ComponentModel.DataAnnotations;

namespace AISEP.BLL.DTOs.Requests
{
    public class ReviewStartupRequest
    {
        [Required]
        public ApprovalStatus Status { get; set; }

        [MaxLength(1000)]
        public string? Reason { get; set; }
    }
}
