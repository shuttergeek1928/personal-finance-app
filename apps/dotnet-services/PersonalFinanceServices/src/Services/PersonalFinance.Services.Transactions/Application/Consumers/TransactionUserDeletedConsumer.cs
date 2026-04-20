using MassTransit;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.Services.Transactions.Infrastructure.Data;
using PersonalFinance.Shared.Events.Events;

namespace PersonalFinance.Services.Transactions.Application.Consumers
{
    public class TransactionUserDeletedConsumer : IConsumer<UserDeletedEvent>
    {
        private readonly TransactionDbContext _context;
        private readonly ILogger<TransactionUserDeletedConsumer> _logger;

        public TransactionUserDeletedConsumer(TransactionDbContext context, ILogger<TransactionUserDeletedConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserDeletedEvent> context)
        {
            var userId = context.Message.UserId;
            _logger.LogInformation("Purging all Transactions for User: {UserId}", userId);

            var userTransactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .ToListAsync();

            if (userTransactions.Any())
            {
                _context.Transactions.RemoveRange(userTransactions);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Purged {Count} transactions for user {UserId}", userTransactions.Count, userId);
            }
        }
    }
}
