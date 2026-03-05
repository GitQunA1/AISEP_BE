using AISEP.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace AISEP.DTOs
{
    public class ReviewStartupDto
    {
        [Required]
        public ApprovalStatus Status { get; set; }

        [MaxLength(1000)]
        public string? Reason { get; set; }
    }
}
