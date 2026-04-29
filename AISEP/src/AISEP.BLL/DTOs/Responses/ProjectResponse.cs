namespace AISEP.BLL.DTOs.Responses
{
    public class ProjectResponse
    {
        public int ProjectId { get; set; }
        public int StartupId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string? ProjectImageUrl { get; set; }
        public string? ShortDescription { get; set; }
        public int? StageOptionId { get; set; }
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
        public List<string> Industries { get; set; } = [];
        public int ViewCount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        //public DateTime? PublishedAt { get; set; }

        // Approval / rejection info
        public int? ApprovedById { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public int? RejectedById { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? RejectionReason { get; set; }
        public int? StartupPotentialScore { get; set; }
        public int FollowerCount { get; set; }
        public bool IsFollowedByCurrentUser { get; set; }
        public bool IsConnectionRequestedByCurrentInvestor { get; set; }
        public int? AssignedAdvisorId { get; set; }
        public string? AssignedAdvisorName { get; set; }
        public decimal? AssignedAdvisorHourlyRate { get; set; }
        public decimal? AssignedAdvisorRating { get; set; }
        public List<string> AssignedAdvisorIndustries { get; set; } = [];
    }
}
