namespace AISEP.BLL.DTOs.Responses
{
    public class ProjectAssignedAdvisorResponse
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public int AdvisorId { get; set; }
        public string AdvisorName { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
    }
}
