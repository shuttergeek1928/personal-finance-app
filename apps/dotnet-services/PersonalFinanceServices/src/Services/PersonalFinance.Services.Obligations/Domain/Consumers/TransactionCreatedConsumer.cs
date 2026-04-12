using MassTransit;

using Microsoft.EntityFrameworkCore;

using PersonalFinance.Services.Obligations.Infrastructure.Data;
using PersonalFinance.Shared.Common.Domain.ValueObjects;
using PersonalFinance.Shared.Events.Events;

namespace PersonalFinance.Services.Obligations.Domain.Consumers
{
    public class TransactionCreatedConsumer :
        IConsumer<IncomeTransactionCreatedEvent>,
        IConsumer<ExpenseTransactionCreatedEvent>,
        IConsumer<TransferTransactionCreatedEvent>
    {
        private readonly ILogger<TransactionCreatedConsumer> _logger;
        private readonly ObligationDbContext _dbContext;

        public TransactionCreatedConsumer(
            ILogger<TransactionCreatedConsumer> logger,
            ObligationDbContext dbContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task Consume(ConsumeContext<IncomeTransactionCreatedEvent> context)
        {
            var txEvent = context.Message;
            if (txEvent.CreditCardId == null) return;

            _logger.LogInformation("Processing Credit Card Income/Refund: {TransactionId} for card {CardId}", txEvent.TransactionId, txEvent.CreditCardId);

            var card = await _dbContext.CreditCards.FirstOrDefaultAsync(c => c.Id == txEvent.CreditCardId);
            if (card != null)
            {
                card.PayBill(new Money(txEvent.Amount, txEvent.Currency));
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Credit card {CardId} updated (Refund/Income)", txEvent.CreditCardId);
            }
        }

        public async Task Consume(ConsumeContext<ExpenseTransactionCreatedEvent> context)
        {
            var txEvent = context.Message;
            _logger.LogInformation("CONSUMER REACHED: ExpenseTransactionCreatedEvent for Transaction {TransactionId}, Card {CardId}", txEvent.TransactionId, txEvent.CreditCardId);

            if (txEvent.CreditCardId == null)
            {
                _logger.LogWarning("Skipping consumption as CreditCardId is null for Transaction {TransactionId}", txEvent.TransactionId);
                return;
            }

            var card = await _dbContext.CreditCards.FirstOrDefaultAsync(c => c.Id == txEvent.CreditCardId);
            if (card != null)
            {
                card.Charge(new Money(txEvent.Amount, txEvent.Currency));
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Credit card {CardId} updated (Expense)", txEvent.CreditCardId);
            }
        }

        public async Task Consume(ConsumeContext<TransferTransactionCreatedEvent> context)
        {
            var txEvent = context.Message;

            // Handle From Credit Card (Spend/Cash Withdrawal)
            if (txEvent.FromCreditCardId != null)
            {
                _logger.LogInformation("Processing Transfer FROM Credit Card: {TransactionId} for card {CardId}", txEvent.TransactionId, txEvent.FromCreditCardId);
                var card = await _dbContext.CreditCards.FirstOrDefaultAsync(c => c.Id == txEvent.FromCreditCardId);
                if (card != null)
                {
                    card.Charge(new Money(txEvent.Amount, txEvent.Currency));
                }
            }

            // Handle To Credit Card (Bill Payment)
            if (txEvent.ToCreditCardId != null)
            {
                _logger.LogInformation("Processing Transfer TO Credit Card: {TransactionId} for card {CardId}", txEvent.TransactionId, txEvent.ToCreditCardId);
                var card = await _dbContext.CreditCards.FirstOrDefaultAsync(c => c.Id == txEvent.ToCreditCardId);
                if (card != null)
                {
                    card.PayBill(new Money(txEvent.Amount, txEvent.Currency));
                }
            }

            if (txEvent.FromCreditCardId != null || txEvent.ToCreditCardId != null)
            {
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
