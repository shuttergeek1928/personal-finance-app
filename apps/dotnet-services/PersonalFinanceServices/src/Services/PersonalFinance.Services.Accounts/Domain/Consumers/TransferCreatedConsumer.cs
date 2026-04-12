using MassTransit;

using PersonalFinance.Services.Accounts.Infrastructure.Data;
using PersonalFinance.Shared.Common.Domain.ValueObjects;
using PersonalFinance.Shared.Events.Events;

namespace PersonalFinance.Services.Accounts.Domain.Consumers
{
    public class TransferCreatedConsumer : IConsumer<TransferTransactionCreatedEvent>
    {
        private readonly ILogger<TransferCreatedConsumer> _logger;
        private readonly AccountDbContext _accountDbContext;
        private readonly IPublishEndpoint _publishEndpoint;

        public TransferCreatedConsumer(
            ILogger<TransferCreatedConsumer> logger,
            AccountDbContext accountDbContext,
            IPublishEndpoint publishEndpoint)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accountDbContext = accountDbContext ?? throw new ArgumentNullException(nameof(accountDbContext));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        }

        public async Task Consume(ConsumeContext<TransferTransactionCreatedEvent> context)
        {
            var transferEvent = context.Message;
            _logger.LogInformation("Processing TransferTransactionCreated: {TransactionId} from {FromAccountId} to {ToAccountId}",
                transferEvent.TransactionId, transferEvent.FromAccountId, transferEvent.ToAccountId);

            using var transaction = await _accountDbContext.Database.BeginTransactionAsync();

            try
            {
                // 1. Withdraw from source account
                var sourceAccount = _accountDbContext.Accounts.FirstOrDefault(a => a.Id == transferEvent.FromAccountId)
                    ?? throw new InvalidOperationException($"Source account {transferEvent.FromAccountId} not found.");

                var sourcePrevBalance = sourceAccount.Balance.Amount;
                sourceAccount.Withdraw(new Money(transferEvent.Amount, transferEvent.Currency));

                // 2. Deposit to destination account
                var destAccount = _accountDbContext.Accounts.FirstOrDefault(a => a.Id == transferEvent.ToAccountId)
                    ?? throw new InvalidOperationException($"Destination account {transferEvent.ToAccountId} not found.");

                var destPrevBalance = destAccount.Balance.Amount;
                destAccount.Deposit(new Money(transferEvent.Amount, transferEvent.Currency));

                await _accountDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // 3. Publish balance updated events
                await _publishEndpoint.Publish(new AccountBalanceUpdatedEvent
                {
                    AccountId = sourceAccount.Id,
                    UserId = transferEvent.UserId,
                    NewBalance = sourceAccount.Balance.Amount,
                    PreviousBalance = sourcePrevBalance,
                    Currency = transferEvent.Currency,
                    UpdateReason = "Transfer Out"
                });

                await _publishEndpoint.Publish(new AccountBalanceUpdatedEvent
                {
                    AccountId = destAccount.Id,
                    UserId = transferEvent.UserId,
                    NewBalance = destAccount.Balance.Amount,
                    PreviousBalance = destPrevBalance,
                    Currency = transferEvent.Currency,
                    UpdateReason = "Transfer In"
                });

                _logger.LogInformation("Transfer transaction {TransactionId} processed successfully", transferEvent.TransactionId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error processing TransferTransactionCreated: {TransactionId}", transferEvent.TransactionId);
                throw;
            }
        }
    }
}
