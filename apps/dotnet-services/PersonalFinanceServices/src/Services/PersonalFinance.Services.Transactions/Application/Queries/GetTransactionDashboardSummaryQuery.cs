using MediatR;
using PersonalFinance.Services.Transactions.Application.DataTransferObjects.Response;
using System;

namespace PersonalFinance.Services.Transactions.Application.Queries
{
    public class GetTransactionDashboardSummaryQuery : IRequest<ApiResponse<TransactionDashboardSummaryDto>>
    {
        public Guid UserId { get; set; }
        
        // Allowed values: "THIS_MONTH", "LAST_3_MONTHS", "THIS_YEAR", "ALL_TIME"
        public string? Period { get; set; } 
    }
}
