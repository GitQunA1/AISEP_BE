namespace AISEP.DTOs
{
    public class ReviewDto
    {
        //public Guid Id { get; set; }

        public Guid AdvisorId { get; set; }

        //public Guid ReviewerId { get; set; }

        public int Rating { get; set; }

        public string? ReviewContent { get; set; }

        //public DateTime CreatedAt { get; set; }
    }
    public class ReviewResponseDto
    {
        public Guid Id { get; set; }
        public string AdvisorName { get; set; } = string.Empty;
        public string ReviewerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? ReviewContent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
