using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PersonalFinance.Services.Accounts.Domain.Entities;
using PersonalFinance.Shared.Common.Infrastructure;

namespace PersonalFinance.Services.Accounts.Infrastructure.Data
{
    public class AccountDbContext : BaseDbContext
    {
        public AccountDbContext(DbContextOptions<AccountDbContext> options, IMediator mediator)
            : base(options, mediator)
        {
        }

        public DbSet<Account> Accounts => Set<Account>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccountDbContext).Assembly);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.PendingModelChangesWarning));
        }
    }
}