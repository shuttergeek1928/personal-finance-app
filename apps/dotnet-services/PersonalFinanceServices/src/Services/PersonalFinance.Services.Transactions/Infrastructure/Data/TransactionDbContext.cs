using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PersonalFinance.Services.Transactions.Domain.Entities;
using PersonalFinance.Shared.Common.Infrastructure;

namespace PersonalFinance.Services.Transactions.Infrastructure.Data
{
    public class TransactionDbContext : BaseDbContext
    {
        public TransactionDbContext(DbContextOptions<TransactionDbContext> options, IMediator mediator)
            : base(options, mediator)
        {
        }

        public DbSet<Transaction> Transactions => Set<Transaction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TransactionDbContext).Assembly);
            ConfigureEntityFilters(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.PendingModelChangesWarning));
        }

        private void ConfigureEntityFilters(ModelBuilder modelBuilder)
        {
        }
    }
}