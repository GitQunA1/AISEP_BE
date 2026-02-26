namespace AISEP.DTOs
{
    public class FollowStartupDto
    {
        public Guid StartupId { get; set; }
    }

    public class StartupFollowerResponseDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public Guid StartupId { get; set; }
        public string StartupName { get; set; } = string.Empty;
        public DateTime FollowedAt { get; set; }
    }

    public class FollowedStartupDto
    {
        public Guid StartupId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? Industry { get; set; }
        public DateTime FollowedAt { get; set; }
    }
}
