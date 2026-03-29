namespace AISEP.BLL.DTOs.Responses
{
    public class FollowedStartupResponse
    {
        public int StartupId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? Industry { get; set; }
        public DateTime FollowedAt { get; set; }
    }
}
