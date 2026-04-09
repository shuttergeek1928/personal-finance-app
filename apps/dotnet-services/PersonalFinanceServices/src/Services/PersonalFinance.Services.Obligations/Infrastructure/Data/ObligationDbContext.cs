using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PersonalFinance.Services.Obligations.Domain.Entities;
using PersonalFinance.Shared.Common.Infrastructure;

namespace PersonalFinance.Services.Obligations.Infrastructure.Data
{
    public class ObligationDbContext : BaseDbContext
    {
        public ObligationDbContext(DbContextOptions<ObligationDbContext> options, IMediator mediator)
            : base(options, mediator)
        {
        }

        public DbSet<Liability> Liabilities => Set<Liability>();
        public DbSet<Subscription> Subscriptions => Set<Subscription>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ObligationDbContext).Assembly);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.PendingModelChangesWarning));
        }
    }
}
