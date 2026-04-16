using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalFinance.Services.UserManagement.Domain.Entities;

namespace PersonalFinance.Services.UserManagement.Infrastructure.Data.Configuration
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Token)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(t => t.CreatedByIp)
                .HasMaxLength(50);

            builder.Property(t => t.RevokedByIp)
                .HasMaxLength(50);

            builder.Property(t => t.ReplacedByToken)
                .HasMaxLength(200);

            builder.HasIndex(t => t.UserId);
        }
    }
}
