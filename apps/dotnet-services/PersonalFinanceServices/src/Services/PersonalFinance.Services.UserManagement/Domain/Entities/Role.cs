using PersonalFinance.Shared.Common.Domain;

namespace PersonalFinance.Services.UserManagement.Domain.Entities
{
    public class Role : BaseEntity
    {
        private readonly List<UserRole> _userRoles = new();

        private Role() { } // EF Core

        public Role(string name, string? description = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description;
        }

        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }

        // Navigation property
        public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

        public void UpdateDetails(string name, string? description)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description;
        }
    }
}