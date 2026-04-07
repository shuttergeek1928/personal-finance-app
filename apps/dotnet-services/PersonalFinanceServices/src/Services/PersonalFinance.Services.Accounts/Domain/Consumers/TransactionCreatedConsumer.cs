using MassTransit;
using PersonalFinance.Services.Accounts.Infrastructure.Data;
using PersonalFinance.Shared.Common.Domain.ValueObjects;
using PersonalFinance.Shared.Events.Events;

namespace PersonalFinance.Services.Accounts.Domain.Consumers
{
    public class TransactionCreatedConsumer : 
        IConsumer<IncomeTransactionCreatedEvent>,
        IConsumer<ExpenseTransactionCreatedEvent>
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

        public async Task Consume(ConsumeContext<IncomeTransactionCreatedEvent> context)
        {
            var transactionEvent = context.Message;
            _logger.LogInformation("Processing IncomeTransactionCreated: {TransactionId}", transactionEvent.TransactionId);

            try
            {
                var account = _accountDbContext.Accounts.FirstOrDefault(a => a.Id == transactionEvent.AccountId) 
                    ?? throw new InvalidOperationException($"Account {transactionEvent.AccountId} not found.");

                var previousBalance = account.Balance.Amount;
                account.Deposit(new Money(transactionEvent.Amount, transactionEvent.Currency));

                await _accountDbContext.SaveChangesAsync();

                await _publishEndpoint.Publish(new AccountBalanceUpdatedEvent
                {
                    AccountId = account.Id,
                    UserId = transactionEvent.UserId,
                    NewBalance = account.Balance.Amount,
                    PreviousBalance = previousBalance,
                    Currency = transactionEvent.Currency,
                    UpdateReason = "Income Transaction"
                });

                _logger.LogInformation("Income transaction {TransactionId} processed successfully", transactionEvent.TransactionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing IncomeTransactionCreated: {TransactionId}", transactionEvent.TransactionId);
                throw;
            }
        }

        public async Task Consume(ConsumeContext<ExpenseTransactionCreatedEvent> context)
        {
            var transactionEvent = context.Message;
            _logger.LogInformation("Processing ExpenseTransactionCreated: {TransactionId}", transactionEvent.TransactionId);

            try
            {
                var account = _accountDbContext.Accounts.FirstOrDefault(a => a.Id == transactionEvent.AccountId) 
                    ?? throw new InvalidOperationException($"Account {transactionEvent.AccountId} not found.");

                var previousBalance = account.Balance.Amount;
                account.Withdraw(new Money(transactionEvent.Amount, transactionEvent.Currency));

                await _accountDbContext.SaveChangesAsync();

                await _publishEndpoint.Publish(new AccountBalanceUpdatedEvent
                {
                    AccountId = account.Id,
                    UserId = transactionEvent.UserId,
                    NewBalance = account.Balance.Amount,
                    PreviousBalance = previousBalance,
                    Currency = transactionEvent.Currency,
                    UpdateReason = "Expense Transaction"
                });

                _logger.LogInformation("Expense transaction {TransactionId} processed successfully", transactionEvent.TransactionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing ExpenseTransactionCreated: {TransactionId}", transactionEvent.TransactionId);
                throw;
            }
        }
    }
}
