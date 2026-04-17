using MassTransit;
using MediatR;
using PersonalFinance.Services.Accounts.Application.Commands;
using PersonalFinance.Shared.Common.Domain.ValueObjects;
using PersonalFinance.Shared.Events.Events;

namespace PersonalFinance.Services.Accounts.Application.Consumers
{
    public class TransactionCreatedConsumer : 
        IConsumer<IncomeTransactionCreatedEvent>,
        IConsumer<ExpenseTransactionCreatedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TransactionCreatedConsumer> _logger;

        public TransactionCreatedConsumer(IMediator mediator, ILogger<TransactionCreatedConsumer> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<IncomeTransactionCreatedEvent> context)
        {
            var msg = context.Message;
            if (msg.AccountId == null) return;

            _logger.LogInformation("Updating balance for account {AccountId} due to income transaction {TransactionId}", msg.AccountId, msg.TransactionId);

            var command = new UpdateBalanceCommand(
                msg.AccountId.Value, 
                new Money(msg.Amount, msg.Currency), 
                true
            );

            await _mediator.Send(command);
        }

        public async Task Consume(ConsumeContext<ExpenseTransactionCreatedEvent> context)
        {
            var msg = context.Message;
            if (msg.AccountId == null) return;

            _logger.LogInformation("Updating balance for account {AccountId} due to expense transaction {TransactionId}", msg.AccountId, msg.TransactionId);

            var command = new UpdateBalanceCommand(
                msg.AccountId.Value, 
                new Money(msg.Amount, msg.Currency), 
                false
            );

            await _mediator.Send(command);
        }
    }
}
