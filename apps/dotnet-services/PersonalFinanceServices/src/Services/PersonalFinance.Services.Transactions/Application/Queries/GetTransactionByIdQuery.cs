using AutoMapper;

using MediatR;

using Microsoft.EntityFrameworkCore;

using PersonalFinance.Services.Transactions.Application.Common;
using PersonalFinance.Services.Transactions.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Transactions.Application.DTOs;
using PersonalFinance.Services.Transactions.Application.Mappings;
using PersonalFinance.Services.Transactions.Infrastructure.Data;

namespace PersonalFinance.Services.Transactions.Application.Queries
{
    public class GetTransactionByIdQuery : IRequest<ApiResponse<TransactionTransferObject>>
    {
        public Guid Id { get; set; }

        public GetTransactionByIdQuery(Guid id)
        {
            Id = id;
        }
    }

    public class GetTransactionByIdQueryHandler : BaseRequestHandler<GetTransactionByIdQuery, ApiResponse<TransactionTransferObject>>
    {
        public GetTransactionByIdQueryHandler(TransactionDbContext context, ILogger<GetTransactionByIdQueryHandler> logger, IMapper mapper)
            : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<TransactionTransferObject>> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
        {
            var transaction = await Context.Transactions.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

            if (transaction == null)
            {
                Logger.LogError("Transaction with ID {transactionId} not found", request.Id);
                return ApiResponse<TransactionTransferObject>.ErrorResult("Transaction not found");
            }

            var transactionDto = transaction.ToDto(Mapper);
            return ApiResponse<TransactionTransferObject>.SuccessResult(transactionDto);
        }
    }
}