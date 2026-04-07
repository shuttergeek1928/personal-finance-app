using AutoMapper;
using MassTransit;
using MediatR;
using PersonalFinance.Services.Transactions.Application.Common;
using PersonalFinance.Services.Transactions.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Transactions.Application.DTOs;
using PersonalFinance.Services.Transactions.Domain.Entities;
using PersonalFinance.Services.Transactions.Infrastructure.Data;
using PersonalFinance.Shared.Common.Domain.ValueObjects;
using PersonalFinance.Shared.Events.Events;
using PersonalFinance.Services.Transactions.Application.Mappings;

namespace PersonalFinance.Services.Transactions.Application.Commands
{
    public class CreateExpenseTransactionCommand : IRequest<ApiResponse<TransactionTransferObject>>
    {
        public Guid UserId { get; set; }
        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public TransactionType Type { get; set; } = TransactionType.Expense;
        public string Description { get; set; }
        public string Category { get; set; } = "Expense";
        public DateTime TransactionDate { get; set; }
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
        public string? RejectionReason { get; set; }
    }

    public class CreateExpenseTransactionCommandHandler : BaseRequestHandler<CreateExpenseTransactionCommand, ApiResponse<TransactionTransferObject>>
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public CreateExpenseTransactionCommandHandler(
            TransactionDbContext context,
            IMapper mapper,
            ILogger<CreateExpenseTransactionCommandHandler> logger,
            IPublishEndpoint publishEndpoint) : base(context, logger, mapper)
        {
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        }

        public override async Task<ApiResponse<TransactionTransferObject>> Handle(CreateExpenseTransactionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation("Creating expense transaction for user {UserId} on account {AccountId} with amount {Amount}", request.UserId, request.AccountId, request.Amount);

                var money = new Money(request.Amount, request.Currency);
                var transaction = new Transaction(
                    request.UserId,
                    request.AccountId,
                    money,
                    request.Type,
                    request.Description,
                    request.Category,
                    request.TransactionDate);

                // Approve the transaction by default (validation will happen via balance check in downstream)
                transaction.Approve();

                Context.Transactions.Add(transaction);
                await Context.SaveChangesAsync(cancellationToken);

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
                }, cancellationToken);

                var dto = transaction.ToDto(Mapper);

                Logger.LogInformation("Expense transaction {TransactionId} created and published successfully", transaction.Id);
                return ApiResponse<TransactionTransferObject>.SuccessResult(dto, "Expense transaction recorded successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating expense transaction for user {UserId}", request.UserId);
                return ApiResponse<TransactionTransferObject>.ErrorResult("An error occurred while creating the transaction");
            }
        }
    }
}
