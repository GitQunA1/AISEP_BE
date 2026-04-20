using AISEP.DAL.Enums;

namespace AISEP.BLL.DTOs.Requests
{
    public class AdminCreateUserRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public UserStatus Status { get; set; } = UserStatus.Active;
        public bool IsPremium { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}
