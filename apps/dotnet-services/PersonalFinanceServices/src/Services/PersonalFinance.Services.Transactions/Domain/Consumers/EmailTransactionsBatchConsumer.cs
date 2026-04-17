using AutoMapper;

using MassTransit;

using Microsoft.EntityFrameworkCore;

using PersonalFinance.Services.Transactions.Application.Commands;
using PersonalFinance.Services.Transactions.Domain.Entities;
using PersonalFinance.Services.Transactions.Infrastructure.Data;
using PersonalFinance.Shared.Common.Domain.ValueObjects;
using PersonalFinance.Shared.Events.Events;

namespace PersonalFinance.Services.Transactions.Domain.Consumers
{
    public class EmailTransactionsBatchConsumer : IConsumer<EmailTransactionsBatchEvent>
    {
        private readonly TransactionDbContext _context;
        private readonly ILogger<EmailTransactionsBatchConsumer> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public EmailTransactionsBatchConsumer(
            TransactionDbContext context,
            ILogger<EmailTransactionsBatchConsumer> logger,
            IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<EmailTransactionsBatchEvent> context)
        {
            var message = context.Message;

            _logger.LogInformation("Received {Count} batch transactions for user {UserId}",
                message.Transactions.Count, message.UserId);

            int imported = 0;
            int errors = 0;

            foreach (var txnItem in message.Transactions)
            {
                try
                {
                    // Basic deduplication check
                    if (!string.IsNullOrEmpty(txnItem.ReferenceNumber))
                    {
                        var exists = await _context.Transactions
                            .AnyAsync(t => t.UserId == message.UserId &&
                                           t.ReferenceNumber == txnItem.ReferenceNumber,
                                           context.CancellationToken);

                        if (exists)
                        {
                            _logger.LogInformation("Skipping duplicate transaction {Ref}", txnItem.ReferenceNumber);
                            continue;
                        }
                    }

                    var txnId = Guid.NewGuid();

                    // Create transaction entity
                    var transaction = new Transaction(
                        message.UserId,
                        txnItem.AccountId ?? Guid.Empty,
                        null,
                        new Money(txnItem.Amount, txnItem.Currency),
                        txnItem.TransactionType.ToUpper() == "INCOME" ? TransactionType.Income : TransactionType.Expense,
                        txnItem.Description,
                        txnItem.Category,
                        txnItem.TransactionDate
                    );

                    transaction.SetReferenceNumber(txnItem.ReferenceNumber);

                    // Add to data context
                    _context.Transactions.Add(transaction);

                    // Publish the specific creation event to update balances
                    if (transaction.Type == TransactionType.Income)
                    {
                        await _publishEndpoint.Publish(new IncomeTransactionCreatedEvent
                        {
                            TransactionId = transaction.Id,
                            UserId = transaction.UserId,
                            AccountId = transaction.AccountId,
                            Amount = transaction.Money.Amount,
                            Currency = transaction.Money.Currency,
                            Description = transaction.Description,
                            Category = transaction.Category,
                            TransactionDate = transaction.TransactionDate
                        }, context.CancellationToken);
                    }
                    else
                    {
                        await _publishEndpoint.Publish(new ExpenseTransactionCreatedEvent
                        {
                            TransactionId = transaction.Id,
                            UserId = transaction.UserId,
                            AccountId = transaction.AccountId,
                            Amount = transaction.Money.Amount,
                            Currency = transaction.Money.Currency,
                            Description = transaction.Description,
                            Category = transaction.Category,
                            TransactionDate = transaction.TransactionDate
                        }, context.CancellationToken);
                    }

                    imported++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error importing transaction item for user {UserId}", message.UserId);
                    errors++;
                }
            }

            await _context.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation("Batch import completed for user {UserId}: {Imported} imported, {Errors} errors",
                message.UserId, imported, errors);
        }
    }
}
