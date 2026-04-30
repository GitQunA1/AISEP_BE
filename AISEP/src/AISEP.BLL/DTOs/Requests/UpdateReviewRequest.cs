namespace AISEP.BLL.DTOs.Requests
{
    public class UpdateReviewRequest
    {
        public int Rating { get; set; }
        public string? ReviewContent { get; set; }
    }
}
