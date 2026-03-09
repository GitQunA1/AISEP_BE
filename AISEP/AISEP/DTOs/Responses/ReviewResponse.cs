namespace AISEP.DTOs.Responses
{
    public class ReviewResponse
    {
        public int Id { get; set; }
        public int AdvisorId { get; set; }
        public string? AdvisorName { get; set; }
        public int ReviewerId { get; set; }
        public string? ReviewerName { get; set; }
        public int Rating { get; set; }
        public string? ReviewContent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
