using System.ComponentModel.DataAnnotations;

namespace AISEP.BLL.DTOs.Requests
{
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "Refresh token là bắt buộc.")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
