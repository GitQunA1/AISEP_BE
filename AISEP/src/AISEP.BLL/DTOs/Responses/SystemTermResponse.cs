namespace AISEP.BLL.DTOs.Responses
{
    public class SystemTermResponse
    {
        public int Id { get; set; }
        public string ContentHtml { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
