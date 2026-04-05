namespace AISEP.BLL.DTOs.Responses
{
    public class PostPrResponseDto
    {
        public int PostPrId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int DealId { get; set; }

        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string? ProjectImage { get; set; }
        public int InvestorId { get; set; }
        public string InvestorName { get; set; } = string.Empty;
        public int StartupId { get; set; }
        public string StartupName { get; set; } = string.Empty;
    }
}
