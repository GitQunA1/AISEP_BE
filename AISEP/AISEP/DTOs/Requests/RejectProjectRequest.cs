using System.ComponentModel.DataAnnotations;

namespace AISEP.DTOs.Requests
{
    public class RejectProjectRequest
    {
        [Required(ErrorMessage = "Reason is required when rejecting a project.")]
        public string Reason { get; set; } = string.Empty;
    }
}
