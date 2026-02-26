using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AISEP.Models.Enums;

namespace AISEP.Models.Entities
{
    public class Project
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? FullDescription { get; set; }
        public ProjectStatus Status { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public ICollection<Document> Documents { get; set; } = new List<Document>();
        public ICollection<AIReport> AIReports { get; set; } = new List<AIReport>();
        public ICollection<Deal> Deals { get; set; } = new List<Deal>();
    }
}
