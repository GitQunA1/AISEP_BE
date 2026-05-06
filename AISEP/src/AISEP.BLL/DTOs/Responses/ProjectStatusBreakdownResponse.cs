namespace AISEP.BLL.DTOs.Responses
{
    public class ProjectStatusBreakdownResponse
    {
        public int DraftCount { get; set; }
        public int PendingCount { get; set; }
        public int PublishedCount { get; set; }
        public int RejectedCount { get; set; }
    }
}
