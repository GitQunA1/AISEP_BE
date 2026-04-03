namespace AISEP.BLL.DTOs.Responses
{
    public class NonPremiumProjectResponse
    {
        public int ProjectId { get; set; }
        public int StartupId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? DevelopmentStage { get; set; }
        public string? ProblemStatement { get; set; }
        public string? SolutionDescription { get; set; }
        public string? TargetCustomers { get; set; }
        public string? UniqueValueProposition { get; set; }
        public string? Industry { get; set; }
        public string? ProjectImageUrl { get; set; }
        public int? StartupPotentialScore { get; set; }
        public DateTime CreatedAt { get; set; }
        public int FollowerCount { get; set; }
        public bool IsFollowedByCurrentUser { get; set; }
        public bool IsConnectionRequestedByCurrentInvestor { get; set; }
        public int? AssignedAdvisorId { get; set; }
        public string? AssignedAdvisorName { get; set; }
        //public DateTime? AssignedAt { get; set; }
    }
}
