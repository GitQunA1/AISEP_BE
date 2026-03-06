using System.ComponentModel.DataAnnotations;

namespace AISEP.DTOs
{
    public class RejectProjectDto
    {
        [Required(ErrorMessage = "Reason is required when rejecting a project.")]
        [MaxLength(1000, ErrorMessage = "Reason must not exceed 1000 characters.")]
        public string Reason { get; set; } = string.Empty;
    }
}
