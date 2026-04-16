 using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.Services.Transactions.Application.Common;
using PersonalFinance.Services.Transactions.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Transactions.Application.DTOs;
using PersonalFinance.Services.Transactions.Application.Mappings;
using PersonalFinance.Services.Transactions.Infrastructure.Data;

namespace PersonalFinance.Services.Transactions.Application.Queries
{
    public class GetPaginatedTransactionsQueryHandler : BaseRequestHandler<GetPaginatedTransactionsQuery, PaginatedApiResponse<IEnumerable<TransactionTransferObject>>>
    {
        public GetPaginatedTransactionsQueryHandler(TransactionDbContext context, ILogger<GetPaginatedTransactionsQueryHandler> logger, IMapper mapper)
            : base(context, logger, mapper)
        {
        }

        public override async Task<PaginatedApiResponse<IEnumerable<TransactionTransferObject>>> Handle(GetPaginatedTransactionsQuery request, CancellationToken cancellationToken)
        {
            var query = Context.Transactions.AsNoTracking().Where(t => t.UserId == request.UserId);

            // Filtering by SourceType
            if (!string.IsNullOrEmpty(request.SourceType) && request.SourceType.ToLower() != "all")
            {
                if (request.SourceType.ToLower() == "account")
                {
                    query = query.Where(t => t.AccountId != null && t.AccountId != Guid.Empty);
                }
                else if (request.SourceType.ToLower() == "card")
                {
                    query = query.Where(t => t.CreditCardId != null && t.CreditCardId != Guid.Empty);
                }
            }

            // Filtering by CardId
            if (request.CardId.HasValue && request.CardId.Value != Guid.Empty)
            {
                query = query.Where(t => t.CreditCardId == request.CardId.Value || t.ToCreditCardId == request.CardId.Value);
            }

            // Filter by Transaction Type
            if (request.Type.HasValue)
            {
                query = query.Where(t => (int)t.Type == request.Type.Value);
            }

            // Filtering by SearchQuery
            if (!string.IsNullOrEmpty(request.SearchQuery))
            {
                var lowerSearch = request.SearchQuery.ToLower();
                query = query.Where(t => 
                    (t.Description != null && t.Description.ToLower().Contains(lowerSearch)) || 
                    (t.Category != null && t.Category.ToLower().Contains(lowerSearch)));
            }

            // Filtering by Period
            // Assuming transaction date is stored in a way EF can decipher dates, or as DateTime.
            if (!string.IsNullOrEmpty(request.Period))
            {
                var now = DateTime.UtcNow;
                if (request.Period == "THIS_MONTH")
                {
                    var startOfMonth = new DateTime(now.Year, now.Month, 1);
                    query = query.Where(t => t.TransactionDate >= startOfMonth);
                }
                else if (request.Period == "LAST_3_MONTHS")
                {
                    var startOf3MonthsAgo = new DateTime(now.Year, now.Month, 1).AddMonths(-2);
                    query = query.Where(t => t.TransactionDate >= startOf3MonthsAgo);
                }
                else if (request.Period == "THIS_YEAR")
                {
                    var startOfYear = new DateTime(now.Year, 1, 1);
                    query = query.Where(t => t.TransactionDate >= startOfYear);
                }
            }

            var totalCount = await query.CountAsync(cancellationToken);

            // Sorting and Pagination
            var transactions = await query
                .OrderByDescending(t => t.TransactionDate)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var transactionDtos = transactions.Select(t => t.ToDto(Mapper));

            return PaginatedApiResponse<IEnumerable<TransactionTransferObject>>.SuccessPaginatedResult(
                transactionDtos, request.Page, request.PageSize, totalCount);
        }
    }
}
