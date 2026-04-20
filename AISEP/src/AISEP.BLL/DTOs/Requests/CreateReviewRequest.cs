namespace AISEP.BLL.DTOs.Requests
{
    public class CreateReviewRequest
    {
        public int BookingId { get; set; }
        public int Rating { get; set; }
        public string? ReviewContent { get; set; }
    }
}
