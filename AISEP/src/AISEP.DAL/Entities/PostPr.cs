using AISEP.DAL.Enums;

namespace AISEP.DAL.Entities
{
    public class PostPr
    {
        public int PostPrId { get; set; }
        public int DealId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public PostPrStatus Status { get; set; } = PostPrStatus.Pending;
        public bool IsDelete { get; set; }
        public DateTime? PublishedAt { get; set; }

        // Navigation properties
        public Deal Deal { get; set; } = null!;
    }
}
