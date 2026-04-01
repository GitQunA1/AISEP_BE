using AISEP.DAL.Enums;

namespace AISEP.BLL.DTOs.Requests
{
    public class RegisterRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
       
        public string ConfirmPassword { get; set; } = string.Empty;
        public UserRole Role { get; set; }
    }
}
