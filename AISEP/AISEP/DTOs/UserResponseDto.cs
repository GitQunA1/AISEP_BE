using AISEP.Models.Enums;

namespace AISEP.DTOs
{
    public class UserResponseDto
    {
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public UserRole Role { get; set; }
        public UserStatus Status { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime? DateOfBirth { get; set; }
        //public DateTime CreatedAt { get; set; }
        public string? PhoneNumber { get; set; }
        //public string? Address { get; set; }
    }
}
