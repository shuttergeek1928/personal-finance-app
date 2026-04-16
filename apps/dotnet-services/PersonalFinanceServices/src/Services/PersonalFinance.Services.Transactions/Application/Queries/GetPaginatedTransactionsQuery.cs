using MediatR;
using PersonalFinance.Services.Transactions.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Transactions.Application.DTOs;

namespace PersonalFinance.Services.Transactions.Application.Queries
{
    public class GetPaginatedTransactionsQuery : IRequest<PaginatedApiResponse<IEnumerable<TransactionTransferObject>>>
    {
        public Guid UserId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        
        // Filters
        public string? SourceType { get; set; } // "account", "card", "all"
        public Guid? CardId { get; set; }
        public string? SearchQuery { get; set; }
        public string? Period { get; set; } // "THIS_MONTH", "LAST_3_MONTHS", "THIS_YEAR" etc...
        public int? Type { get; set; } // 0 = Income, 1 = Expense, 2 = Transfer
    }
}
