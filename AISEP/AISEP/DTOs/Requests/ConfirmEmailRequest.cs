using System.ComponentModel.DataAnnotations;

namespace AISEP.DTOs.Requests
{
    public class ConfirmEmailRequest
    {
        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; } = string.Empty;
    }
}
