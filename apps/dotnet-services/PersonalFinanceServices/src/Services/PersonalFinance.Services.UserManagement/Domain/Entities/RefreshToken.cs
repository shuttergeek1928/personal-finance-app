using PersonalFinance.Shared.Common.Domain;

namespace PersonalFinance.Services.UserManagement.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public string Token { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public string? CreatedByIp { get; private set; }
        public DateTime? RevokedAt { get; private set; }
        public string? RevokedByIp { get; private set; }
        public string? ReplacedByToken { get; private set; }
        public bool IsFullyActive => RevokedAt == null && !IsExpired;

        public Guid UserId { get; private set; }
        public User User { get; private set; } = null!;

        private RefreshToken() { } // EF Core

        public RefreshToken(string token, DateTime expiresAt, Guid userId, string? createdByIp = null)
        {
            Id = Guid.NewGuid();
            Token = token ?? throw new ArgumentNullException(nameof(token));
            ExpiresAt = expiresAt;
            UserId = userId;
            CreatedAt = DateTime.UtcNow;
            CreatedByIp = createdByIp;
            IsActive = true;
        }

        public void Revoke(string? revokedByIp = null, string? replacedByToken = null)
        {
            RevokedAt = DateTime.UtcNow;
            RevokedByIp = revokedByIp;
            ReplacedByToken = replacedByToken;
            IsActive = false;
        }
    }
}
