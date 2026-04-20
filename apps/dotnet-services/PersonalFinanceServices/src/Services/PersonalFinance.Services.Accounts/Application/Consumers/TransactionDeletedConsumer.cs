using MassTransit;
using MediatR;
using PersonalFinance.Services.Accounts.Application.Commands;
using PersonalFinance.Shared.Common.Domain.ValueObjects;
using PersonalFinance.Shared.Events.Events;

namespace PersonalFinance.Services.Accounts.Application.Consumers
{
    public class TransactionDeletedConsumer : 
        IConsumer<TransactionDeletedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TransactionDeletedConsumer> _logger;

        public TransactionDeletedConsumer(IMediator mediator, ILogger<TransactionDeletedConsumer> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<TransactionDeletedEvent> context)
        {
            var msg = context.Message;
            if (msg.AccountId == null) return;

            _logger.LogInformation("Reversing balance update for account {AccountId} due to transaction deletion {TransactionId} (Type: {Type})", 
                msg.AccountId, msg.TransactionId, msg.TransactionType);

            // If it was Income, we need to Withdraw to undo.
            // If it was Expense, we need to Deposit to undo.
            bool isDeposit = msg.TransactionType.Equals("Expense", StringComparison.OrdinalIgnoreCase);

            var command = new UpdateBalanceCommand(
                msg.AccountId.Value, 
                new Money(msg.Amount, "INR"), // Assume INR or get from message if added
                isDeposit
            );

            await _mediator.Send(command);
        }
    }
}
