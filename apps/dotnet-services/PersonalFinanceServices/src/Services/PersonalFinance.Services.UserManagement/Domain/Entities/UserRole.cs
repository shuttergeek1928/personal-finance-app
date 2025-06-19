using PersonalFinance.Services.UserManagement.Domain.Entities;
using PersonalFinance.Shared.Common.Domain;

namespace PersonalFinance.Services.UserManagement.Domain.Entities
{
    public class UserRole : BaseEntity
    {
        private UserRole() { } // EF Core

        public UserRole(Guid userId, Guid roleId)
        {
            UserId = userId;
            RoleId = roleId;
        }

        public Guid UserId { get; private set; }
        public Guid RoleId { get; private set; }

        // Navigation properties
        public User User { get; private set; } = null!;
        public Role Role { get; private set; } = null!;
    }
}