namespace AISEP.Models.Entities
{
    public class InvestorAIAnalysis
    {
        public int AnalysisId { get; set; }
        public int InvestorId { get; set; }
        public int ProjectId { get; set; }
        public string? AnalysisJson { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Investor Investor { get; set; } = null!;
        public Project Project { get; set; } = null!;
    }
}
