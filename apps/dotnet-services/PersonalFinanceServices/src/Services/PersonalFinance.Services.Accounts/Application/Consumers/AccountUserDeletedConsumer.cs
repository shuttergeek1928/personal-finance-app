using MassTransit;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.Services.Accounts.Infrastructure.Data;
using PersonalFinance.Shared.Events.Events;

namespace PersonalFinance.Services.Accounts.Application.Consumers
{
    public class AccountUserDeletedConsumer : IConsumer<UserDeletedEvent>
    {
        private readonly AccountDbContext _context;
        private readonly ILogger<AccountUserDeletedConsumer> _logger;

        public AccountUserDeletedConsumer(AccountDbContext context, ILogger<AccountUserDeletedConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserDeletedEvent> context)
        {
            var userId = context.Message.UserId;
            _logger.LogInformation("Purging all data for User: {UserId} from Accounts service", userId);

            var userAccounts = await _context.Accounts
                .Where(a => a.UserId == userId)
                .ToListAsync();

            if (userAccounts.Any())
            {
                _context.Accounts.RemoveRange(userAccounts);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Purged {Count} accounts for user {UserId}", userAccounts.Count, userId);
            }
        }
    }
}
