using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalFinance.Services.Obligations.Domain.Entities;

namespace PersonalFinance.Services.Obligations.Infrastructure.Data.Configuration
{
    public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
    {
        public void Configure(EntityTypeBuilder<Subscription> builder)
        {
            builder.ToTable("Subscriptions");

            builder.Property(s => s.Name)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(s => s.Provider)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(s => s.Type)
                .IsRequired();

            builder.Property(s => s.BillingCycle)
                .IsRequired();

            builder.Property(s => s.StartDate)
                .IsRequired();

            builder.Property(s => s.NextBillingDate)
                .IsRequired();

            builder.Property(s => s.AutoRenew)
                .HasDefaultValue(true);

            builder.Property(s => s.UserId)
                .IsRequired();

            builder.HasIndex(s => s.UserId)
                .HasDatabaseName("IX_Subscription_UserId");

            // Configure Money value object
            builder.OwnsOne(s => s.Amount, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("Amount")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                money.Property(m => m.Currency)
                    .HasColumnName("Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

            // Ignore domain events for EF Core
            builder.Ignore(s => s.DomainEvents);
        }
    }
}
