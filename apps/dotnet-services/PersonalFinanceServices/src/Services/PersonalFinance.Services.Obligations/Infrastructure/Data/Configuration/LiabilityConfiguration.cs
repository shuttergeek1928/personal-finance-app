using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalFinance.Services.Obligations.Domain.Entities;

namespace PersonalFinance.Services.Obligations.Infrastructure.Data.Configuration
{
    public class LiabilityConfiguration : IEntityTypeConfiguration<Liability>
    {
        public void Configure(EntityTypeBuilder<Liability> builder)
        {
            builder.ToTable("Liabilities");

            builder.Property(l => l.Name)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(l => l.LenderName)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(l => l.Type)
                .IsRequired();

            builder.Property(l => l.InterestRate)
                .HasColumnType("decimal(8,4)")
                .IsRequired();

            builder.Property(l => l.TenureMonths)
                .IsRequired();

            builder.Property(l => l.StartDate)
                .IsRequired();

            builder.Property(l => l.EndDate)
                .IsRequired();

            builder.Property(l => l.UserId)
                .IsRequired();

            builder.HasIndex(l => l.UserId)
                .HasDatabaseName("IX_Liability_UserId");

            builder.HasIndex(l => l.Type)
                .HasDatabaseName("IX_Liability_Type");

            builder.Property(l => l.IsNoCostEmi)
                .HasDefaultValue(false);

            builder.Property(l => l.AccountId)
                .HasColumnName("AccountId")
                .IsRequired(false);

            builder.Property(l => l.CreditCardId)
                .HasColumnName("CreditCardId")
                .IsRequired(false);

            builder.HasIndex(l => l.CreditCardId)
                .HasDatabaseName("IX_Liability_CreditCardId");

            // Configure Money value objects
            builder.OwnsOne(l => l.PrincipalAmount, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("PrincipalAmount")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                money.Property(m => m.Currency)
                    .HasColumnName("PrincipalCurrency")
                    .HasMaxLength(3)
                    .HasDefaultValue("INR")
                    .IsRequired();
            });

            builder.OwnsOne(l => l.OutstandingBalance, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("OutstandingBalanceAmount")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                money.Property(m => m.Currency)
                    .HasColumnName("OutstandingBalanceCurrency")
                    .HasMaxLength(3)
                    .HasDefaultValue("INR")
                    .IsRequired();
            });

            builder.OwnsOne(l => l.EmiAmount, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("EmiAmount")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                money.Property(m => m.Currency)
                    .HasColumnName("EmiCurrency")
                    .HasMaxLength(3)
                    .HasDefaultValue("INR")
                    .IsRequired();
            });

            builder.OwnsOne(l => l.ProcessingFee, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("ProcessingFeeAmount")
                    .HasColumnType("decimal(18,2)");

                money.Property(m => m.Currency)
                    .HasColumnName("ProcessingFeeCurrency")
                    .HasMaxLength(3)
                    .HasDefaultValue("INR");
            });

            // If CreditCard service allows setting foreign key constraints, we can explicitly add one here
            // but for microservices we often store it just as an ID. In this bounded context, CreditCard exists.
            builder.HasOne(l => l.CreditCard)
                .WithMany()
                .HasForeignKey(l => l.CreditCardId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // Ignore domain events for EF Core
            builder.Ignore(l => l.DomainEvents);
        }
    }
}
