using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AISEP.Models.Enums;

namespace AISEP.Models.Entities
{
    public class Project
    {
        public int ProjectId { get; set; }
        public int StartupId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? FullDescription { get; set; }
        public ProjectStatus Status { get; set; }

        // Navigation properties
        public Startup Startup { get; set; } = null!;
        public ICollection<Document> Documents { get; set; } = new List<Document>();
        public ICollection<StartupAIAnalysis> StartupAIAnalyses { get; set; } = new List<StartupAIAnalysis>();
        public ICollection<InvestorAIAnalysis> InvestorAIAnalyses { get; set; } = new List<InvestorAIAnalysis>();
        public ICollection<Deal> Deals { get; set; } = new List<Deal>();
    }
}
