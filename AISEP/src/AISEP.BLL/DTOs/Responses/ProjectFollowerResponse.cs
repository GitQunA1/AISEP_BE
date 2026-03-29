namespace AISEP.BLL.DTOs.Responses
{
    public class ProjectFollowerResponse
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public DateTime FollowedAt { get; set; }
    }
}
