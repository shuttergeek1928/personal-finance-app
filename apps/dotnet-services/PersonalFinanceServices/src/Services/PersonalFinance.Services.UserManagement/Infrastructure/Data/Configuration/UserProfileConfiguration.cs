using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalFinance.Services.UserManagement.Domain.Entities;

namespace PersonalFinance.Services.UserManagement.Infrastructure.Data.Configuration
{
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
