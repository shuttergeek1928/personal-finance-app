using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using PersonalFinance.Services.EmailIngestion.Domain.Entities;
using PersonalFinance.Shared.Common.Infrastructure;

namespace PersonalFinance.Services.EmailIngestion.Infrastructure.Data
{
    public class EmailIngestionDbContext : BaseDbContext
    {
        public EmailIngestionDbContext(DbContextOptions<EmailIngestionDbContext> options, IMediator mediator)
            : base(options, mediator)
        {
        }

        public DbSet<ProcessedEmail> ProcessedEmails => Set<ProcessedEmail>();
        public DbSet<ParsedTransaction> ParsedTransactions => Set<ParsedTransaction>();
        public DbSet<SyncState> SyncStates => Set<SyncState>();
        public DbSet<GmailUserToken> UserTokens => Set<GmailUserToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EmailIngestionDbContext).Assembly);

            // ProcessedEmail configuration
            modelBuilder.Entity<ProcessedEmail>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.GmailMessageId }).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.Property(e => e.GmailMessageId).HasMaxLength(256).IsRequired();
                entity.Property(e => e.ThreadId).HasMaxLength(256);
                entity.Property(e => e.Subject).HasMaxLength(1000);
                entity.Property(e => e.SenderEmail).HasMaxLength(500);
                entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            });

            // ParsedTransaction configuration
            modelBuilder.Entity<ParsedTransaction>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ProcessedEmailId);
                entity.HasIndex(e => new { e.UserId, e.Status });
                entity.HasIndex(e => new { e.ReferenceNumber, e.TransactionDate, e.Amount })
                    .HasFilter("[ReferenceNumber] IS NOT NULL");

                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.Currency).HasMaxLength(10).HasDefaultValue("INR");
                entity.Property(e => e.TransactionType).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Category).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.MerchantName).HasMaxLength(500);
                entity.Property(e => e.ReferenceNumber).HasMaxLength(256);
                entity.Property(e => e.Source).HasMaxLength(50).HasDefaultValue("Gmail");

                entity.HasOne(e => e.ProcessedEmail)
                    .WithMany()
                    .HasForeignKey(e => e.ProcessedEmailId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // SyncState configuration
            modelBuilder.Entity<SyncState>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.Category }).IsUnique();
                entity.Property(e => e.Category).HasMaxLength(100).IsRequired();
            });

            // GmailUserToken configuration
            modelBuilder.Entity<GmailUserToken>(entity =>
            {
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.Property(e => e.AccessToken).IsRequired();
                entity.Property(e => e.RefreshToken).IsRequired();
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.PendingModelChangesWarning));
        }
    }
}
