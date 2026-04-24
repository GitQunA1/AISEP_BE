using AISEP.DAL.Enums;

namespace AISEP.BLL.DTOs.Responses
{
    public class AdminUserResponse
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public UserRole Role { get; set; }
        public UserStatus Status { get; set; }
        public bool IsPremium { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
