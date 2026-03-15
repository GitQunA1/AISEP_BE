namespace AISEP.BLL.DTOs.Responses
{
    public class StartupResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? CompanyName { get; set; }
        public string? LogoUrl { get; set; }
        public string? Founder { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CountryCity { get; set; }
        public string? Website { get; set; }
        public string? Industry { get; set; }
        public string? BusinessLicenseUrl { get; set; }
        public string ApprovalStatus { get; set; } = string.Empty;
        public int FollowerCount { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Approval / Rejection info
        public int? ApprovedById { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public int? RejectedById { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? RejectionReason { get; set; }
    }
}
