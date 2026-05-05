namespace AISEP.BLL.DTOs.Responses
{
    public class DueDiligenceTemplateResponse
    {
        public int Id { get; set; }
        public string ContentJson { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
