using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalFinance.Services.Accounts.Domain.Entities;

namespace PersonalFinance.Services.Accounts.Infrastructure.Data.Configurations
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.ToTable("Accounts");

            builder.Property(a => a.AccountNumber)
                 .HasColumnName("AccountNumber")
                 .HasMaxLength(16)
                 .IsRequired();

            builder.HasIndex(a => a.AccountNumber)
                .IsUnique()
                .HasDatabaseName("IX_AccountNumber");

            builder.Property(a => a.Description)
                .HasMaxLength(256);

            builder.Property(a => a.Name)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(a => a.Type)
                .IsRequired();

            builder.Property(a => a.UserId)
                .IsRequired();

            // Configure Balance, Money value object  
            builder.OwnsOne(a => a.Balance, balance =>
            {
                balance.Property(b => b.Amount)
                    .HasColumnName("BalanceAmount")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                balance.Property(b => b.Currency)
                    .HasColumnName("BalanceCurrency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

            // Ignore domain events for EF Core  
            builder.Ignore(u => u.DomainEvents);
        }
    }
}