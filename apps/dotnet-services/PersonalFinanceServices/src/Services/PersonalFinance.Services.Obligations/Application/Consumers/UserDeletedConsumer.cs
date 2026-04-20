using MassTransit;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.Services.Obligations.Infrastructure.Data;
using PersonalFinance.Shared.Events.Events;

namespace PersonalFinance.Services.Obligations.Application.Consumers
{
    public class UserDeletedConsumer : IConsumer<UserDeletedEvent>
    {
        private readonly ObligationDbContext _context;
        private readonly ILogger<UserDeletedConsumer> _logger;

        public UserDeletedConsumer(ObligationDbContext context, ILogger<UserDeletedConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserDeletedEvent> context)
        {
            var userId = context.Message.UserId;
            _logger.LogInformation("Purging all data for User: {UserId} from Obligations service", userId);

            // 1. Delete Liabilities (Loans/EMIs)
            var liabilities = await _context.Liabilities
                .Where(o => o.UserId == userId)
                .ToListAsync();
            if (liabilities.Any()) _context.Liabilities.RemoveRange(liabilities);

            // 2. Delete Subscriptions
            var subscriptions = await _context.Subscriptions
                .Where(o => o.UserId == userId)
                .ToListAsync();
            if (subscriptions.Any()) _context.Subscriptions.RemoveRange(subscriptions);

            // 3. Delete Credit Cards
            var creditCards = await _context.CreditCards
                .Where(o => o.UserId == userId)
                .ToListAsync();
            if (creditCards.Any()) _context.CreditCards.RemoveRange(creditCards);

            await _context.SaveChangesAsync();
            _logger.LogInformation("Full purge completed for user {UserId} in Obligations service", userId);
        }
    }
}
