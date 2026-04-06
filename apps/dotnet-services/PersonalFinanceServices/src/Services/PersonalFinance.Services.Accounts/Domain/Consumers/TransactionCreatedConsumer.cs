using MassTransit;
using PersonalFinance.Services.Accounts.Infrastructure.Data;
using PersonalFinance.Shared.Common.Domain.ValueObjects;
using PersonalFinance.Shared.Events.Events;

namespace PersonalFinance.Services.Accounts.Domain.Consumers
{
    public class TransactionCreatedConsumer : IConsumer<IncomeTransactionCreatedEvent>
    {
        private readonly ILogger<TransactionCreatedConsumer> _logger;
        private readonly AccountDbContext _accountDbContext;
        private readonly IPublishEndpoint _publishEndpoint;

        public TransactionCreatedConsumer(
            ILogger<TransactionCreatedConsumer> logger,
            AccountDbContext accountDbContext,
            IPublishEndpoint publishEndpoint)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accountDbContext = accountDbContext ?? throw new ArgumentNullException(nameof(accountDbContext));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        }

        // This consumer is responsible for handling transaction creation events
        public async Task Consume(ConsumeContext<IncomeTransactionCreatedEvent> context)
        {
            var transactionEvent = context.Message;

            _logger.LogInformation(
                "Received IncomeTransactionCreated event: TransactionId={TransactionId}, UserId={UserId}, Amount={Amount}, Currency={Currency}",
                transactionEvent.TransactionId,
                transactionEvent.UserId,
                transactionEvent.Amount,
                transactionEvent.Currency);

            try
            {
                await ProcessTransaction(transactionEvent);
                
                _logger.LogInformation(
                    "Processed IncomeTransactionCreated event successfully: TransactionId={TransactionId}, UserId={UserId}",
                    transactionEvent.TransactionId,
                    transactionEvent.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing IncomeTransactionCreated event: TransactionId={TransactionId}, UserId={UserId}",
                    transactionEvent.TransactionId,
                    transactionEvent.UserId);
                throw;
            }
        }

        private async Task ProcessTransaction(IncomeTransactionCreatedEvent transactionEvent)
        {
            var account = _accountDbContext.Accounts.FirstOrDefault(a => a.Id == transactionEvent.AccountId) ?? throw new InvalidOperationException($"Account with ID {transactionEvent.AccountId} not found.");

            account.Deposit(new Money(transactionEvent.Amount, transactionEvent.Currency));

            await _accountDbContext.SaveChangesAsync();

            await _publishEndpoint.Publish(new AccountBalanceUpdatedEvent
            {
                AccountId = account.Id,
                UserId = transactionEvent.UserId,
                NewBalance = account.Balance.Amount,
                PreviousBalance = account.Balance.Amount - transactionEvent.Amount,
                Currency = transactionEvent.Currency,
                UpdateReason = "Income Transaction Created"
            });
        }
    }
}
