namespace AISEP.DTOs.Requests
{
    public class CreateReviewRequest
    {
        public int AdvisorId { get; set; }
        public int Rating { get; set; }
        public string? ReviewContent { get; set; }
    }
}
