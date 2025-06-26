using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PersonalFinance.Services.UserManagement.Domain.Entities;
using PersonalFinance.Shared.Common.Infrastructure;

namespace PersonalFinance.Services.UserManagement.Infrastructure.Data
{
    public class UserManagementDbContext : BaseDbContext
    {
        public UserManagementDbContext(DbContextOptions<UserManagementDbContext> options, IMediator mediator)
            : base(options, mediator)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserManagementDbContext).Assembly);
            ConfigureEntityFilters(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.PendingModelChangesWarning));
        }

        private void ConfigureEntityFilters(ModelBuilder modelBuilder)
        {
            // User entity filter
            modelBuilder.Entity<User>()
                .HasQueryFilter(u => u.IsActive);

            // Role entity filter  
            modelBuilder.Entity<Role>()
                .HasQueryFilter(r => r.IsActive);

            // UserProfile doesn't need IsActive filter (it follows User)
            // UserRole doesn't need IsActive filter (it's a junction table)
        }
    }
}