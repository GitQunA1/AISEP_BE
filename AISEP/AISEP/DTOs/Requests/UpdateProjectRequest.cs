using AISEP.Models.Enums;

namespace AISEP.DTOs.Requests
{
    public class UpdateProjectRequest
    {
        public string ProjectName { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public DevelopmentStage? DevelopmentStage { get; set; }
        public string? ProblemStatement { get; set; }
        public string? SolutionDescription { get; set; }
        public string? TargetCustomers { get; set; }
        public string? UniqueValueProposition { get; set; }
        public decimal? MarketSize { get; set; }
        public string? BusinessModel { get; set; }
        public decimal? Revenue { get; set; }
        public string? Competitors { get; set; }
        public string? TeamMembers { get; set; }
        public string? KeySkills { get; set; }
        public string? TeamExperience { get; set; }
    }
}
