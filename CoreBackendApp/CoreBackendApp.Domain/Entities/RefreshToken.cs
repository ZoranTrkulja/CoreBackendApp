namespace CoreBackendApp.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public string TokenHash { get; set; } = null!;
        public Guid UserId { get; set; }
        public DateTime ExipresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RevokedAt { get; set; }
        public string? ReplacedByTokenHash { get; set; }
        public string CreatedByIp { get; set; } = null!;
        public bool IsExpired => DateTime.UtcNow >= ExipresAt;
        public bool IsActive => RevokedAt == null && !IsExpired;
    }
}