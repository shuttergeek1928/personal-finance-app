using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalFinance.Services.Transactions.Domain.Entities;
using System;

namespace PersonalFinance.Services.Transactions.Infrastructure.Data.Configurations
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transactions");

            // Configure Money, Money value object  
            builder.OwnsOne(t => t.Money, balance =>
            {
                balance.Property(b => b.Amount)
                    .HasColumnName("Amount")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                balance.Property(b => b.Currency)
                    .HasColumnName("Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

            builder.Property(a => a.Type)
                .IsRequired();

            builder.Property(a => a.UserId)
                .IsRequired();

            builder.Property(a => a.Description)
                .HasMaxLength(256);

            builder.Property(a => a.Category)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(a => a.TransactionDate)
                .IsRequired();

            builder.Property(a => a.RejectionReason)
                .HasMaxLength(500)
                .IsRequired(false); // Nullable field

            builder.Property(a => a.AccountId)
                .IsRequired();

            builder.Property(a => a.Status)
                .IsRequired();

            // Ignore domain events for EF Core
            builder.Ignore(u => u.DomainEvents);
        }
    }
}