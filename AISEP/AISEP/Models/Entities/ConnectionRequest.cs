using System.ComponentModel.DataAnnotations;
using AISEP.Models.Enums;

namespace AISEP.Models.Entities
{
    public class ConnectionRequest
    {
        public int ConnectionRequestId { get; set; }
        public int InvestorId { get; set; }
        public int ProjectId { get; set; }
        public ConnectionRequestStatus Status { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? ResponseDate { get; set; }

        [MaxLength(1000)]
        public string? Message { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        [MaxLength(1000)]
        public string? ResponseMessage { get; set; }

        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Investor Investor { get; set; } = null!;
        public Project Project { get; set; } = null!;
        public ICollection<PostPr> PostPrs { get; set; } = new List<PostPr>();
    }
}
