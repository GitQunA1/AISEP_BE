using System.ComponentModel.DataAnnotations.Schema;

namespace AISEP.Models.Entities
{
    public class SuccessStory
    {
        public int Id { get; set; }
        public int ConnectionId { get; set; }
        public string? Content { get; set; }
        public bool StartupApproved { get; set; }
        public bool InvestorApproved { get; set; }

        // Navigation properties
        public ConnectionRequest ConnectionRequest { get; set; } = null!;
    }
}
