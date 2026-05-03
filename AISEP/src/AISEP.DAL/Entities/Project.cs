using AISEP.DAL.Enums;

namespace AISEP.DAL.Entities
{
    public class Project
    {
        public int ProjectId { get; set; }
        public int StartupId { get; set; }
        public int IndustryOptionId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string? ProjectImageUrl { get; set; }
        public string? ShortDescription { get; set; }
        public int? StageOptionId { get; set; }
        public string? ProblemStatement { get; set; }
        public string? SolutionDescription { get; set; }
        public string? TargetCustomers { get; set; }
        public string? UniqueValueProposition { get; set; }
        public string? BusinessModel { get; set; }
        public string? Competitors { get; set; }
        public ProjectStatus Status { get; set; } = ProjectStatus.Draft;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? ApprovedById { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public int? RejectedById { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? RejectionReason { get; set; }

        public ProjectScorecard? Scorecard { get; set; }

        // Navigation properties
        public Startup Startup { get; set; } = null!;
        public IndustryOption? IndustryOption { get; set; }
        public StageOption? StageOption { get; set; }
        public ICollection<Document> Documents { get; set; } = new List<Document>();
        public StartupAIAnalysis? StartupAIAnalysis { get; set; }
        public ICollection<InvestorAIAnalysis> InvestorAIAnalyses { get; set; } = new List<InvestorAIAnalysis>();
        public ICollection<ProjectFollower> Followers { get; set; } = new List<ProjectFollower>();
        public ICollection<UnlockedProject> UnlockedProjects { get; set; } = new List<UnlockedProject>();
        public ICollection<ConnectionRequest> ConnectionRequests { get; set; } = new List<ConnectionRequest>();
        public ICollection<Deal> Deals { get; set; } = new List<Deal>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<ProjectAdvisorAssignment> ProjectAdvisorAssignments { get; set; } = new List<ProjectAdvisorAssignment>();
    }
}
