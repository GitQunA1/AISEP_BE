using System.ComponentModel.DataAnnotations;

namespace AISEP.BLL.DTOs.Requests
{
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "Refresh token is required.")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
