using MassTransit;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.Services.EmailIngestion.Infrastructure.Data;
using PersonalFinance.Shared.Events.Events;

namespace PersonalFinance.Services.EmailIngestion.Application.Consumers
{
    public class EmailIngestionUserDeletedConsumer : IConsumer<UserDeletedEvent>
    {
        private readonly EmailIngestionDbContext _context;
        private readonly ILogger<EmailIngestionUserDeletedConsumer> _logger;

        public EmailIngestionUserDeletedConsumer(EmailIngestionDbContext context, ILogger<EmailIngestionUserDeletedConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserDeletedEvent> context)
        {
            var userId = context.Message.UserId;
            _logger.LogInformation("Purging all data for User: {UserId} from Email Ingestion service", userId);

            // 1. Delete parsed transactions
            var transactions = await _context.ParsedTransactions
                .Where(t => t.UserId == userId)
                .ToListAsync();
            
            if (transactions.Any())
            {
                _context.ParsedTransactions.RemoveRange(transactions);
            }

            // 2. Delete processed emails
            var emails = await _context.ProcessedEmails
                .Where(e => e.UserId == userId)
                .ToListAsync();
            
            if (emails.Any())
            {
                _context.ProcessedEmails.RemoveRange(emails);
            }

            // 3. Delete sync states
            var syncStates = await _context.SyncStates
                .Where(s => s.UserId == userId)
                .ToListAsync();
            
            if (syncStates.Any())
            {
                _context.SyncStates.RemoveRange(syncStates);
            }

            // 4. Delete user tokens
            var tokens = await _context.UserTokens
                .Where(t => t.UserId == userId)
                .ToListAsync();
            
            if (tokens.Any())
            {
                _context.UserTokens.RemoveRange(tokens);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Full purge completed for user {UserId} in Email Ingestion service", userId);
        }
    }
}
