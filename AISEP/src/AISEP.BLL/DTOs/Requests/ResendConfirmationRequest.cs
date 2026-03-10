using System.ComponentModel.DataAnnotations;

namespace AISEP.BLL.DTOs.Requests
{
    public class ResendConfirmationRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
    }
}
