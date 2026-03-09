using AISEP.Models.Enums;

namespace AISEP.DTOs.Responses
{
    public class UserResponse
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public UserRole Role { get; set; }
        public UserStatus Status { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
