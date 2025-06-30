// PersonalFinance.Services.Transactions/Application/Commands/RegisterUserCommand.cs
using AutoMapper;
using MediatR;
using PersonalFinance.Services.Transactions.Application.Common;
using PersonalFinance.Services.Transactions.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Transactions.Application.DTOs;
using PersonalFinance.Services.Transactions.Application.Mappings;
using PersonalFinance.Services.Transactions.Domain.Entities;
using PersonalFinance.Services.Transactions.Infrastructure.Data;
using PersonalFinance.Shared.Common.Domain.ValueObjects;

namespace PersonalFinance.Services.Transactions.Application.Commands
{
    public class CreateIncomeTransactionCommand : IRequest<ApiResponse<TransactionTransferObject>>
    {
        public Guid UserId { get; set; }
        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public TransactionType Type { get; set; }
        public string Description { get; set; }
        public string Category { get; set; } = "Income";
        public DateTime TransactionDate { get; set; }
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
        public string? RejectionReason { get; set; }
    }

    public class CreateIncomeTransactionCommandHandler : BaseRequestHandler<CreateIncomeTransactionCommand, ApiResponse<TransactionTransferObject>>
    {
        public CreateIncomeTransactionCommandHandler(
            TransactionDbContext context,
            IMapper mapper,
            ILogger<CreateIncomeTransactionCommandHandler> logger) : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<TransactionTransferObject>> Handle(CreateIncomeTransactionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation("Creating the transaction for user id: {userId} on account {accountId} with amount {amount}", request.UserId, request.AccountId, request.Amount);

                var money = new Money(request.Amount, request.Currency);
                var transaction = new Transaction(
                    request.UserId,
                    request.AccountId,
                    money,
                    request.Type,
                    request.Description,
                    request.Category,
                    request.TransactionDate);

                // Approve the transaction by default
                transaction.Approve();

                Context.Transactions.Add(transaction);
                await Context.SaveChangesAsync(cancellationToken);

                var UserTransferObject = transaction.ToDto(Mapper);

                Logger.LogInformation("Income transaction {TransactionId} created and published successfully", transaction.Id);
                return ApiResponse<TransactionTransferObject>.SuccessResult(UserTransferObject, "User registered successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating income transaction for user {UserId}", request.UserId);
                return ApiResponse<TransactionTransferObject>.ErrorResult("An error occurred while creating the transaction");
            }
        }
    }
}