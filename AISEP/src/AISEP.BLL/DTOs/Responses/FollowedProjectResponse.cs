namespace AISEP.BLL.DTOs.Responses
{
    public class FollowedProjectResponse
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string? ProjectImageUrl { get; set; }
        public List<string> Industries { get; set; } = [];
        public DateTime FollowedAt { get; set; }
    }
}
