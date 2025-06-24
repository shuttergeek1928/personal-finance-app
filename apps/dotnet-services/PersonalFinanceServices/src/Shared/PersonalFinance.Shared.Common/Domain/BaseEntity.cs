using System.ComponentModel.DataAnnotations;

namespace PersonalFinance.Shared.Common.Domain
{
    public abstract class BaseEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
    public abstract class AuditableEntity : BaseEntity
    {
        public string? CreatedBy { get; set; }

        public string? UpdatedBy { get; set; }

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public abstract class UserOwnedEntity : AuditableEntity
    {
        [Required]
        public Guid UserId { get; set; }
    }
}
