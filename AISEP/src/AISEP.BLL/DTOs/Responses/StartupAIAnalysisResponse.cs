namespace AISEP.BLL.DTOs.Responses
{
    public class StartupAIAnalysisResponse
    {
        public int    EvaluationId      { get; set; }
        public int    ProjectId         { get; set; }
        public int?   PotentialScore    { get; set; }
        public int?   ChaosScore        { get; set; }
        public string? AnalysisJson     { get; set; }
        public bool?  IsEligibleStartup { get; set; }
        public string? EligibilityReason { get; set; }
        public DateTime CreatedAt       { get; set; }
    }
}
