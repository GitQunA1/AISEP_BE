namespace AISEP.BLL.DTOs.Responses
{
    public class AIAnalysisReportDto
    {
        public decimal TotalBaseScore { get; set; }
        public decimal TotalAIAdjustmentScore { get; set; }
        public decimal TotalFinalScore { get; set; }
        public List<AuditedItemDto> AuditedItems { get; set; } = [];
        public List<string> Strengths { get; set; } = [];
        public List<string> Weaknesses { get; set; } = [];
        public List<string> Advice { get; set; } = [];
    }

    public class AuditedItemDto
    {
        public string Criteria { get; set; } = string.Empty;
        public decimal MaxScore { get; set; }
        public decimal BaseScore { get; set; }
        public string Finding { get; set; } = string.Empty;
        public decimal Adjustment { get; set; }
        public decimal FinalScore { get; set; }
    }
}
