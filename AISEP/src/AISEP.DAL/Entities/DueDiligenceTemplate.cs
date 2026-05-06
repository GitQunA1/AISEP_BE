namespace AISEP.DAL.Entities
{
    public class DueDiligenceTemplate
    {
        public int Id { get; set; }
        public string ContentJson { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
