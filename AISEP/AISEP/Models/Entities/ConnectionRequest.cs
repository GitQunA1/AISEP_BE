using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AISEP.Models.Enums;

namespace AISEP.Models.Entities
{
    public class ConnectionRequest
    {
        public int Id { get; set; }
     public int InvestorId { get; set; }
     public int StartupId { get; set; }
     public ConnectionRequestStatus Status { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? ResponseDate { get; set; }
        public string? Message { get; set; }
        public string? Reason { get; set; }

          // Navigation properties
        public Investor Investor { get; set; } = null!;
        public Startup Startup { get; set; } = null!;
        public ICollection<SuccessStory> SuccessStories { get; set; } = new List<SuccessStory>();
    }
}
