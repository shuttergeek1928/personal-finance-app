using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalFinance.Services.UserManagement.Domain.Entities;

namespace PersonalFinance.Services.UserManagement.Infrastructure.Data.Configuration
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles");

            builder.Property(r => r.Name)
                .HasMaxLength(256)
                .IsRequired();

            builder.HasIndex(r => r.Name)
                .IsUnique();

            builder.Property(r => r.Description)
                .HasMaxLength(500);

            builder.HasMany(r => r.UserRoles)
                .WithOne(ur => ur.Role)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed default roles
            builder.HasData(
                new
                {
                    Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
                    Name = "Admin",
                    Description = "Administrator with full access",
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), // ✅ Static date
                    UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), // ✅ Static date
                    IsActive = true,
                    RowVersion = new byte[0] // Empty byte array for row version
                },
                new
                {
                    Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
                    Name = "User",
                    Description = "Regular user",
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), // ✅ Static date
                    UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), // ✅ Static date
                    IsActive = true,
                    RowVersion = new byte[0] // Empty byte array for row version
                }
            );
        }
    }
}
