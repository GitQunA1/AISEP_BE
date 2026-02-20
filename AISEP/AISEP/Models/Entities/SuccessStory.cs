using System.ComponentModel.DataAnnotations.Schema;

namespace AISEP.Models.Entities
{
    public class SuccessStory
    {
        public Guid Id { get; set; }
        public Guid ConnectionId { get; set; }
        public string? Content { get; set; }
        public bool StartupApproved { get; set; }
        public bool InvestorApproved { get; set; }

        // Navigation properties
        public ConnectionRequest ConnectionRequest { get; set; } = null!;
    }
}
