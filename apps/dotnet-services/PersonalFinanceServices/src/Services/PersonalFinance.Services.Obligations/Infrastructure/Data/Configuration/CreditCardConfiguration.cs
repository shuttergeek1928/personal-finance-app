using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalFinance.Services.Obligations.Domain.Entities;

namespace PersonalFinance.Services.Obligations.Infrastructure.Data.Configuration
{
    public class CreditCardConfiguration : IEntityTypeConfiguration<CreditCard>
    {
        public void Configure(EntityTypeBuilder<CreditCard> builder)
        {
            builder.ToTable("CreditCards");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.BankName)
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(c => c.CardName)
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(c => c.Last4Digits)
                .HasMaxLength(4)
                .IsFixedLength()
                .IsRequired();

            builder.Property(c => c.ExpiryMonth)
                .IsRequired();

            builder.Property(c => c.ExpiryYear)
                .IsRequired();

            builder.Property(c => c.NetworkProvider)
                .IsRequired();

            builder.Property(c => c.UserId)
                .IsRequired();

            builder.OwnsOne(c => c.TotalLimit, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("TotalLimitAmount")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                money.Property(m => m.Currency)
                    .HasColumnName("TotalLimitCurrency")
                    .HasMaxLength(3)
                    .HasDefaultValue("INR")
                    .IsRequired();
            });

            builder.OwnsOne(c => c.OutstandingAmount, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("OutstandingAmount")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                money.Property(m => m.Currency)
                    .HasColumnName("OutstandingCurrency")
                    .HasMaxLength(3)
                    .HasDefaultValue("INR")
                    .IsRequired();
            });

            builder.HasIndex(c => c.UserId)
                .HasDatabaseName("IX_CreditCard_UserId");

            builder.Ignore(c => c.DomainEvents);
        }
    }
}
