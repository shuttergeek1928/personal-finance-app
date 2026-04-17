using MassTransit;
using MediatR;
using PersonalFinance.Services.Transactions.Application.Commands;
using PersonalFinance.Shared.Events.Events;

namespace PersonalFinance.Services.Transactions.Application.Consumers
{
    public class TransactionCreatedConsumer : 
        IConsumer<ExternalTransactionConfirmedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TransactionCreatedConsumer> _logger;

        public TransactionCreatedConsumer(IMediator mediator, ILogger<TransactionCreatedConsumer> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ExternalTransactionConfirmedEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation("Processing ExternalTransactionConfirmedEvent from {Source} for User: {UserId}", "Gmail", msg.UserId);

            if (msg.TransactionType.Equals("Income", StringComparison.OrdinalIgnoreCase))
            {
                var command = new CreateIncomeTransactionCommand
                {
                    UserId = msg.UserId,
                    AccountId = msg.AccountId,
                    Amount = msg.Amount,
                    Currency = msg.Currency,
                    Description = msg.Description,
                    Category = msg.Category,
                    TransactionDate = msg.TransactionDate,
                    Type = Domain.Entities.TransactionType.Income
                };
                await _mediator.Send(command);
            }
            else
            {
                var command = new CreateExpenseTransactionCommand
                {
                    UserId = msg.UserId,
                    AccountId = msg.AccountId,
                    Amount = msg.Amount,
                    Currency = msg.Currency,
                    Description = msg.Description,
                    Category = msg.Category,
                    TransactionDate = msg.TransactionDate,
                    Type = Domain.Entities.TransactionType.Expense
                };
                await _mediator.Send(command);
            }
        }
    }
}
