using System;
using System.Collections.Generic;

namespace PersonalFinance.Services.Transactions.Application.DataTransferObjects.Response
{
    public class CategorySpendingDto
    {
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
    }

    public class HeatmapItemDto
    {
        public string DateStr { get; set; }
        public decimal Amount { get; set; }
        public double Intensity { get; set; }
    }

    public class MonthlyFlowDto
    {
        public string Month { get; set; }
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
        public decimal Net { get; set; }
    }

    public class TransactionDashboardSummaryDto
    {
        public Guid UserId { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal NetCashFlow { get; set; }
        
        public decimal LastPeriodIncome { get; set; }
        public decimal LastPeriodExpense { get; set; }
        
        public double IncomeTrendPercentage { get; set; }
        public double ExpenseTrendPercentage { get; set; }

        public IEnumerable<CategorySpendingDto> SpendingByCategory { get; set; } = new List<CategorySpendingDto>();
        public IEnumerable<HeatmapItemDto> ExpenseHeatmap { get; set; } = new List<HeatmapItemDto>();
        public IEnumerable<MonthlyFlowDto> MonthlyFlow { get; set; } = new List<MonthlyFlowDto>();
    }
}
