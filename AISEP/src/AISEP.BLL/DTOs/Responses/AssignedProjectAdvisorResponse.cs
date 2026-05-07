namespace AISEP.BLL.DTOs.Responses
{
    public class AssignedProjectAdvisorResponse
    {
        public int AdvisorId { get; set; }
        public string AdvisorName { get; set; } = string.Empty;
        public decimal? HourlyRate { get; set; }
        public decimal? Rating { get; set; }
        public List<string> Industries { get; set; } = [];
        public DateTime AssignedAt { get; set; }
    }
}
