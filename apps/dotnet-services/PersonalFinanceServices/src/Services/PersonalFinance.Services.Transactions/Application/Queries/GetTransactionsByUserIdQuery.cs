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
    public class GetTransactionsByUserIdQuery : IRequest<ApiResponse<IEnumerable<TransactionTransferObject>>>
    {
        public Guid UserId { get; set; }

        public GetTransactionsByUserIdQuery(Guid userId)
        {
            UserId = userId;
        }
    }

    public class GetTransactionsByUserIdQueryHandler : BaseRequestHandler<GetTransactionsByUserIdQuery, ApiResponse<IEnumerable<TransactionTransferObject>>>
    {
        public GetTransactionsByUserIdQueryHandler(TransactionDbContext context, ILogger<GetTransactionsByUserIdQueryHandler> logger, IMapper mapper)
            : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<IEnumerable<TransactionTransferObject>>> Handle(GetTransactionsByUserIdQuery request, CancellationToken cancellationToken)
        {
            var transactions = await Context.Transactions.AsNoTracking()
                .Where(t => t.UserId == request.UserId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync(cancellationToken);

            var transactionDtos = transactions.Select(t => t.ToDto(Mapper));
            return ApiResponse<IEnumerable<TransactionTransferObject>>.SuccessResult(transactionDtos);
        }
    }
}
