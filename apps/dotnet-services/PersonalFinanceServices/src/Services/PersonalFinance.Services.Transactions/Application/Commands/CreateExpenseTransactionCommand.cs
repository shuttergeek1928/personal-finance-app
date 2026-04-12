using AutoMapper;

using MassTransit;

using MediatR;

using PersonalFinance.Services.Transactions.Application.Common;
using PersonalFinance.Services.Transactions.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Transactions.Application.DTOs;
using PersonalFinance.Services.Transactions.Application.Mappings;
using PersonalFinance.Services.Transactions.Domain.Entities;
using PersonalFinance.Services.Transactions.Infrastructure.Data;
using PersonalFinance.Shared.Common.Domain.ValueObjects;
using PersonalFinance.Shared.Events.Events;

namespace PersonalFinance.Services.Transactions.Application.Commands
{
    public class CreateExpenseTransactionCommand : IRequest<ApiResponse<TransactionTransferObject>>
    {
        public Guid UserId { get; set; }
        public Guid? AccountId { get; set; }
        public Guid? CreditCardId { get; set; }
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
        private readonly IRequestClient<CheckBalanceRequest> _checkBalanceClient;

        public CreateExpenseTransactionCommandHandler(
            TransactionDbContext context,
            IMapper mapper,
            ILogger<CreateExpenseTransactionCommandHandler> logger,
            IPublishEndpoint publishEndpoint,
            IRequestClient<CheckBalanceRequest> checkBalanceClient) : base(context, logger, mapper)
        {
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
            _checkBalanceClient = checkBalanceClient ?? throw new ArgumentNullException(nameof(checkBalanceClient));
        }

        public override async Task<ApiResponse<TransactionTransferObject>> Handle(CreateExpenseTransactionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation("Creating expense transaction for user {UserId} on account/card {SourceId} with amount {Amount}",
                    request.UserId, request.AccountId ?? request.CreditCardId, request.Amount);

                // Synchronous balance check using MassTransit Request-Response
                var accountIdToCheck = request.AccountId ?? request.CreditCardId;
                if (accountIdToCheck.HasValue)
                {
                    var balanceCheckResponse = await _checkBalanceClient.GetResponse<CheckBalanceResponse>(new CheckBalanceRequest
                    {
                        AccountId = accountIdToCheck.Value,
                        Amount = request.Amount,
                        TransactionType = "Expense"
                    }, cancellationToken);

                    if (!balanceCheckResponse.Message.HasSufficientFunds)
                    {
                        Logger.LogWarning("Insufficient funds for Account {AccountId}. Balance is {Balance}, Requested amount is {Amount}",
                            accountIdToCheck.Value, balanceCheckResponse.Message.AvailableBalance, request.Amount);

                        var moneyRej = new Money(request.Amount, request.Currency);
                        var transactionRej = new Transaction(
                            request.UserId,
                            request.AccountId,
                            request.CreditCardId,
                            moneyRej,
                            request.Type,
                            request.Description,
                            request.Category,
                            request.TransactionDate);

                        transactionRej.Reject($"Insufficient funds. Available balance: {balanceCheckResponse.Message.AvailableBalance}");

                        Context.Transactions.Add(transactionRej);
                        await Context.SaveChangesAsync(cancellationToken);

                        return ApiResponse<TransactionTransferObject>.SuccessResult(transactionRej.ToDto(Mapper), "Transaction rejected due to insufficient funds.");
                    }
                }

                var money = new Money(request.Amount, request.Currency);
                var transaction = new Transaction(
                    request.UserId,
                    request.AccountId,
                    request.CreditCardId,
                    money,
                    request.Type,
                    request.Description,
                    request.Category,
                    request.TransactionDate);

                transaction.Approve();

                Context.Transactions.Add(transaction);
                await Context.SaveChangesAsync(cancellationToken);

                await _publishEndpoint.Publish(new ExpenseTransactionCreatedEvent
                {
                    TransactionId = transaction.Id,
                    UserId = transaction.UserId,
                    AccountId = transaction.AccountId,
                    CreditCardId = transaction.CreditCardId,
                    Amount = transaction.Money.Amount,
                    Currency = transaction.Money.Currency,
                    Description = transaction.Description,
                    Category = transaction.Category,
                    TransactionDate = transaction.TransactionDate
                }, cancellationToken);

                Logger.LogInformation("Integration event ExpenseTransactionCreatedEvent published for Transaction {TransactionId}", transaction.Id);

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
