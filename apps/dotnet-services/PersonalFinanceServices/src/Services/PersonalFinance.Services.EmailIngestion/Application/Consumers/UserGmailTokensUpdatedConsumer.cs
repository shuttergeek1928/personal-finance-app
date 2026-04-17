using MassTransit;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.Services.EmailIngestion.Domain.Entities;
using PersonalFinance.Services.EmailIngestion.Infrastructure.Data;
using PersonalFinance.Shared.Events.Events;

namespace PersonalFinance.Services.EmailIngestion.Application.Consumers
{
    public class UserGmailTokensUpdatedConsumer : IConsumer<UserGmailTokensUpdatedEvent>
    {
        private readonly EmailIngestionDbContext _dbContext;
        private readonly ILogger<UserGmailTokensUpdatedConsumer> _logger;

        public UserGmailTokensUpdatedConsumer(EmailIngestionDbContext dbContext, ILogger<UserGmailTokensUpdatedConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserGmailTokensUpdatedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Updating local Gmail tokens for user {UserId}", message.UserId);

            var existingToken = await _dbContext.UserTokens
                .FirstOrDefaultAsync(t => t.UserId == message.UserId);

            if (existingToken == null)
            {
                var newToken = new GmailUserToken
                {
                    UserId = message.UserId,
                    Email = message.Email,
                    AccessToken = message.AccessToken,
                    RefreshToken = message.RefreshToken,
                    ExpiresAt = message.ExpiresAt
                };
                _dbContext.UserTokens.Add(newToken);
            }
            else
            {
                existingToken.AccessToken = message.AccessToken;
                existingToken.RefreshToken = message.RefreshToken;
                existingToken.ExpiresAt = message.ExpiresAt;
                existingToken.UpdatedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
