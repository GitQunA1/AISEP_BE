using AISEP.DAL.Enums;

namespace AISEP.BLL.DTOs.Requests
{
    public class AdminUpdateUserRequest
    {
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public UserRole? Role { get; set; }
        public UserStatus? Status { get; set; }
        public bool? IsPremium { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}
