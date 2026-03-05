using AISEP.Models.Enums;

namespace AISEP.DTOs
{
    public class StartupResponseDto
    {
        public int Id { get; set; }
        public string? CompanyName { get; set; }
        public string? LogoUrl { get; set; }
        public string? Founder { get; set; }
        public string? ContactInfo { get; set; }
        public string? CountryCity { get; set; }
        public string? Website { get; set; }
        public string? Industry { get; set; }
        public string? BusinessLicenseUrl { get; set; }
        public string ApprovalStatus { get; set; } = string.Empty;
        public int FollowerCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
