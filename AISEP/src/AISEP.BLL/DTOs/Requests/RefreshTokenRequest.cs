using System.ComponentModel.DataAnnotations;

namespace AISEP.BLL.DTOs.Requests
{
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "Mã làm mới phiên đăng nhập là bắt buộc.")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
