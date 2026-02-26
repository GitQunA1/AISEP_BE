using AISEP.Models.Enums;

namespace AISEP.Models.Entities
{
    public class AIReport
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int? PotentialScore { get; set; }
        public int? ChaosScore { get; set; }
        public string? AnalysisResult { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public AIStatus AiStatus { get; set; }

        // Navigation properties
        public Project Project { get; set; } = null!;
    }
}
