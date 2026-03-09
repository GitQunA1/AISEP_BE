namespace AISEP.Models.Entities
{
    public class ActionLog
    {
        public int ActionLogId { get; set; }
        public int ActorId { get; set; }
        public int? TargetId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User Actor { get; set; } = null!;
    }
}
