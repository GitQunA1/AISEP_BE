using AISEP.DAL.Enums;

namespace AISEP.DAL.Entities
{
    public class Project
    {
        public int ProjectId { get; set; }
        public int StartupId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string? ProjectImageUrl { get; set; }
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
        public Industry Industry { get; set; }
        public int ViewCount { get; set; } = 0;
        public ProjectStatus Status { get; set; } = ProjectStatus.Draft;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? ApprovedById { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public int? RejectedById { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? RejectionReason { get; set; }

        // Navigation properties
        public Startup Startup { get; set; } = null!;
        public ICollection<Document> Documents { get; set; } = new List<Document>();
        public StartupAIAnalysis? StartupAIAnalysis { get; set; }
        public ICollection<InvestorAIAnalysis> InvestorAIAnalyses { get; set; } = new List<InvestorAIAnalysis>();
        public ICollection<UnlockedProject> UnlockedProjects { get; set; } = new List<UnlockedProject>();
        public ICollection<ConnectionRequest> ConnectionRequests { get; set; } = new List<ConnectionRequest>();
        public ICollection<Deal> Deals { get; set; } = new List<Deal>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ProjectAdvisorAssignment? ProjectAdvisorAssignment { get; set; }
    }
}
