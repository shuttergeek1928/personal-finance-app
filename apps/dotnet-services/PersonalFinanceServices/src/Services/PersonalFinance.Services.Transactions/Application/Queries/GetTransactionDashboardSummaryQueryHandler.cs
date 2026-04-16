using MediatR;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.Services.Transactions.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Transactions.Domain.Entities;
using PersonalFinance.Services.Transactions.Infrastructure.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PersonalFinance.Services.Transactions.Application.Queries
{
    public class GetTransactionDashboardSummaryQueryHandler : IRequestHandler<GetTransactionDashboardSummaryQuery, ApiResponse<TransactionDashboardSummaryDto>>
    {
        private readonly TransactionDbContext _dbContext;

        public GetTransactionDashboardSummaryQueryHandler(TransactionDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ApiResponse<TransactionDashboardSummaryDto>> Handle(GetTransactionDashboardSummaryQuery request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            DateTime currentPeriodStart;
            DateTime previousPeriodStart;
            DateTime previousPeriodEnd;

            switch (request.Period?.ToUpperInvariant())
            {
                case "THIS_YEAR":
                    currentPeriodStart = new DateTime(now.Year, 1, 1);
                    previousPeriodStart = currentPeriodStart.AddYears(-1);
                    previousPeriodEnd = currentPeriodStart;
                    break;
                case "LAST_3_MONTHS":
                    currentPeriodStart = now.AddMonths(-3);
                    previousPeriodStart = currentPeriodStart.AddMonths(-3);
                    previousPeriodEnd = currentPeriodStart;
                    break;
                case "ALL_TIME":
                    currentPeriodStart = DateTime.MinValue;
                    previousPeriodStart = DateTime.MinValue;
                    previousPeriodEnd = DateTime.MinValue;
                    break;
                case "THIS_MONTH":
                default:
                    currentPeriodStart = new DateTime(now.Year, now.Month, 1);
                    previousPeriodStart = currentPeriodStart.AddMonths(-1);
                    previousPeriodEnd = currentPeriodStart;
                    break;
            }

            var baseQuery = _dbContext.Transactions
                .Where(t => t.UserId == request.UserId && t.Status != TransactionStatus.Rejected);

            var currentPeriodTransactions = await baseQuery
                .Where(t => t.TransactionDate >= currentPeriodStart)
                .Select(t => new { t.Type, t.Money.Amount, t.Category })
                .ToListAsync(cancellationToken);

            decimal currentIncome = currentPeriodTransactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);

            decimal currentExpense = currentPeriodTransactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            decimal previousIncome = 0;
            decimal previousExpense = 0;

            if (request.Period != "ALL_TIME")
            {
                var prevList = await baseQuery
                    .Where(t => t.TransactionDate >= previousPeriodStart && t.TransactionDate < previousPeriodEnd)
                    .Select(t => new { t.Type, t.Money.Amount })
                    .ToListAsync(cancellationToken);

                previousIncome = prevList
                    .Where(t => t.Type == TransactionType.Income)
                    .Sum(t => t.Amount);

                previousExpense = prevList
                    .Where(t => t.Type == TransactionType.Expense)
                    .Sum(t => t.Amount);
            }

            // Load 12 months for flow and 30 days for heatmap
            var historyStart = now.AddMonths(-12);
            var historyTransactions = await baseQuery
                .Where(t => t.TransactionDate >= historyStart)
                .Select(t => new { t.TransactionDate, t.Type, t.Money.Amount })
                .ToListAsync(cancellationToken);

            var monthlyFlow = historyTransactions
                .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
                .Select(g => new MonthlyFlowDto
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yy"),
                    Income = g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                    Expense = g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount),
                    Net = g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount) - 
                          g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
                })
                .OrderBy(m => DateTime.ParseExact(m.Month, "MMM yy", null))
                .ToList();

            var heatmapStart = now.Date.AddDays(-29);
            var heatmapData = historyTransactions
                .Where(t => t.Type == TransactionType.Expense && t.TransactionDate >= heatmapStart)
                .GroupBy(t => t.TransactionDate.Date)
                .Select(g => new HeatmapItemDto
                {
                    DateStr = g.Key.ToString("yyyy-MM-dd"),
                    Amount = g.Sum(t => t.Amount)
                })
                .ToList();

            var maxAmount = heatmapData.Any() ? heatmapData.Max(h => h.Amount) : 1;
            
            // Fill sparse days to guarantee 30 elements
            var completeHeatmap = Enumerable.Range(0, 30).Select(i => 
            {
                var d = heatmapStart.AddDays(i);
                var match = heatmapData.FirstOrDefault(h => h.DateStr == d.ToString("yyyy-MM-dd"));
                var amount = match?.Amount ?? 0;
                return new HeatmapItemDto
                {
                    DateStr = d.ToString("yyyy-MM-dd"),
                    Amount = amount,
                    Intensity = amount > 0 ? (double)(Math.Max(0.15m, Math.Min(1m, amount / (maxAmount * 0.8m != 0 ? maxAmount * 0.8m : 1m)))) : 0
                };
            }).ToList();

            var summary = new TransactionDashboardSummaryDto
            {
                UserId = request.UserId,
                TotalIncome = currentIncome,
                TotalExpense = currentExpense,
                NetCashFlow = currentIncome - currentExpense,
                LastPeriodIncome = previousIncome,
                LastPeriodExpense = previousExpense,
                IncomeTrendPercentage = CalculateTrend(currentIncome, previousIncome),
                ExpenseTrendPercentage = CalculateTrend(currentExpense, previousExpense),
                SpendingByCategory = currentPeriodTransactions
                    .Where(t => t.Type == TransactionType.Expense && !string.IsNullOrEmpty(t.Category))
                    .GroupBy(t => t.Category)
                    .Select(g => new CategorySpendingDto
                    {
                        Category = g.Key,
                        Amount = g.Sum(t => t.Amount),
                        Currency = "INR" // TODO: Multi-currency aggregation
                    })
                    .OrderByDescending(c => c.Amount)
                    .ToList(),
                MonthlyFlow = monthlyFlow,
                ExpenseHeatmap = completeHeatmap
            };

            return new ApiResponse<TransactionDashboardSummaryDto>
            {
                Success = true,
                Message = "Dashboard summary fetched successfully.",
                Data = summary
            };
        }

        private static double CalculateTrend(decimal current, decimal previous)
        {
            if (previous == 0) return current > 0 ? 100 : 0;
            return (double)((current - previous) / Math.Abs(previous)) * 100;
        }
    }
}
