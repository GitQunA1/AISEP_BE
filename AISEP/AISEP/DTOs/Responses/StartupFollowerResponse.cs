namespace AISEP.DTOs.Responses
{
    public class StartupFollowerResponse
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int StartupId { get; set; }
        public string StartupName { get; set; } = string.Empty;
        public DateTime FollowedAt { get; set; }
    }
}
