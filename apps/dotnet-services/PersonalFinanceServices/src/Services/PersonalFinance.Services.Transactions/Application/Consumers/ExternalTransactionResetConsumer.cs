using MassTransit;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.Services.Transactions.Infrastructure.Data;
using PersonalFinance.Shared.Events.Events;

namespace PersonalFinance.Services.Transactions.Application.Consumers
{
    public class ExternalTransactionResetConsumer : 
        IConsumer<ExternalTransactionResetEvent>
    {
        private readonly TransactionDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<ExternalTransactionResetConsumer> _logger;

        public ExternalTransactionResetConsumer(
            TransactionDbContext context,
            IPublishEndpoint publishEndpoint,
            ILogger<ExternalTransactionResetConsumer> logger)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ExternalTransactionResetEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation("Processing ExternalTransactionResetEvent for Transaction: {TxnId}", msg.ConfirmedTransactionId);

            if (!msg.ConfirmedTransactionId.HasValue)
            {
                _logger.LogWarning("Reset event received without ConfirmedTransactionId. Skipping.");
                return;
            }

            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == msg.ConfirmedTransactionId.Value);

            if (transaction != null)
            {
                // Capture details before deletion for reversal event
                var txnId = transaction.Id;
                var userId = transaction.UserId;
                var accountId = transaction.AccountId;
                var creditCardId = transaction.CreditCardId;
                var amount = transaction.Money.Amount;
                var type = transaction.Type.ToString();

                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted transaction {TxnId} from ledger due to reset.", txnId);

                // Publish deletion event to trigger balance reversal in Accounts service
                await _publishEndpoint.Publish(new TransactionDeletedEvent
                {
                    TransactionId = txnId,
                    UserId = userId,
                    AccountId = accountId,
                    CreditCardId = creditCardId,
                    Amount = amount,
                    TransactionType = type
                });
            }
            else
            {
                _logger.LogWarning("Transaction {TxnId} not found in ledger. Nothing to reset.", msg.ConfirmedTransactionId);
            }
        }
    }
}
