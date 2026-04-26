namespace AISEP.BLL.DTOs.Responses
{
    public class StageOptionResponse
    {
        public int Id { get; set; }
        public string Value { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
