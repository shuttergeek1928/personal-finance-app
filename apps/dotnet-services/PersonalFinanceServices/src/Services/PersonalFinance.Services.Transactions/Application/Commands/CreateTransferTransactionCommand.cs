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
    public class CreateTransferTransactionCommand : IRequest<ApiResponse<TransactionTransferObject>>
    {
        public Guid UserId { get; set; }
        public Guid FromAccountId { get; set; }
        public Guid ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public TransactionType Type { get; set; } = TransactionType.Transfer;
        public string Description { get; set; }
        public string Category { get; set; } = "Transfer";
        public DateTime TransactionDate { get; set; }
    }

    public class CreateTransferTransactionCommandHandler : BaseRequestHandler<CreateTransferTransactionCommand, ApiResponse<TransactionTransferObject>>
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public CreateTransferTransactionCommandHandler(
            TransactionDbContext context,
            IMapper mapper,
            ILogger<CreateTransferTransactionCommandHandler> logger,
            IPublishEndpoint publishEndpoint) : base(context, logger, mapper)
        {
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        }

        public override async Task<ApiResponse<TransactionTransferObject>> Handle(CreateTransferTransactionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation("Creating transfer transaction for user {UserId} from account {FromAccountId} to account {ToAccountId} with amount {Amount}", 
                    request.UserId, request.FromAccountId, request.ToAccountId, request.Amount);

                var money = new Money(request.Amount, request.Currency);
                
                // Record the transaction on the source account
                var transaction = new Transaction(
                    request.UserId,
                    request.FromAccountId,
                    money,
                    request.Type,
                    request.Description,
                    request.Category,
                    request.TransactionDate,
                    request.ToAccountId);

                transaction.Approve();

                Context.Transactions.Add(transaction);
                await Context.SaveChangesAsync(cancellationToken);

                // Publish Transfer event
                await _publishEndpoint.Publish(new TransferTransactionCreatedEvent
                {
                    TransactionId = transaction.Id,
                    UserId = transaction.UserId,
                    FromAccountId = transaction.AccountId,
                    ToAccountId = transaction.ToAccountId!.Value,
                    Amount = transaction.Money.Amount,
                    Currency = transaction.Money.Currency,
                    Description = transaction.Description,
                    Category = transaction.Category,
                    TransactionDate = transaction.TransactionDate
                }, cancellationToken);

                var dto = transaction.ToDto(Mapper);

                Logger.LogInformation("Transfer transaction {TransactionId} created and published successfully", transaction.Id);
                return ApiResponse<TransactionTransferObject>.SuccessResult(dto, "Transfer transaction recorded successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating transfer transaction for user {UserId}", request.UserId);
                return ApiResponse<TransactionTransferObject>.ErrorResult("An error occurred while creating the transfer");
            }
        }
    }
}
