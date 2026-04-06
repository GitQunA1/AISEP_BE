namespace AISEP.BLL.DTOs.Requests
{
    public class CreatePostPrRequest
    {
        public int DealId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
    }
}
