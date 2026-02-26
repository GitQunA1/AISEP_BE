namespace AISEP.Models.Entities
{
  public class RefreshToken
    {
        public int Id { get; set; }
   public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; }
      public DateTime CreatedAt { get; set; }
        public string? CreatedByIp { get; set; }
        public DateTime? RevokedAt { get; set; }
  public string? RevokedByIp { get; set; }
        public string? ReplacedByToken { get; set; }

        // Navigation property
        public User User { get; set; } = null!;

        public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}
