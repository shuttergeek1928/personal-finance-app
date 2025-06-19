using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalFinance.Services.UserManagement.Domain.Entities;
using System;

namespace PersonalFinance.Services.UserManagement.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            // Configure Email value object
            builder.OwnsOne(u => u.Email, email =>
            {
                email.Property(e => e.Value)
                    .HasColumnName("Email")
                    .HasMaxLength(256)
                    .IsRequired();

                email.HasIndex(e => e.Value)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_Email");
            });

            builder.Property(u => u.UserName)
                .HasMaxLength(256)
                .IsRequired();

            builder.HasIndex(u => u.UserName)
                .IsUnique();

            builder.Property(u => u.PasswordHash)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(u => u.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(u => u.LastName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(u => u.PhoneNumber)
                .HasMaxLength(20);

            // One-to-one relationship with UserProfile
            builder.HasOne(u => u.Profile)
                .WithOne(p => p.User)
                .HasForeignKey<UserProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Many-to-many relationship with Roles through UserRole
            builder.HasMany(u => u.UserRoles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ignore domain events for EF Core
            builder.Ignore(u => u.DomainEvents);
        }
    }

    public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
    {
        public void Configure(EntityTypeBuilder<UserProfile> builder)
        {
            builder.ToTable("UserProfiles");

            builder.Property(p => p.Currency)
                .HasMaxLength(3)
                .IsRequired()
                .HasDefaultValue("INR");

            builder.Property(p => p.TimeZone)
                .HasMaxLength(50)
                .IsRequired()
                .HasDefaultValue("UTC");

            builder.Property(p => p.Language)
                .HasMaxLength(10)
                .IsRequired()
                .HasDefaultValue("en-US");

            builder.Property(p => p.Avatar)
                .HasMaxLength(500);

            // Configure JSON column for financial goals
            builder.Property(p => p.FinancialGoals)
                .HasColumnType("nvarchar(max)");
        }
    }
}