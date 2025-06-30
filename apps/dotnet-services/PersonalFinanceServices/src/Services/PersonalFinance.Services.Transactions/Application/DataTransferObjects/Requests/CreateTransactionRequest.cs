using PersonalFinance.Services.Transactions.Domain.Entities;
using PersonalFinance.Shared.Common.Domain.ValueObjects;

namespace PersonalFinance.Services.Transactions.Application.DataTransferObjects.Requests
{
    public class CreateTransactionRequest
    {
        public Guid UserId { get; set; }
        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR"; // Default currency
        public string Description { get; set; }
        public string Category { get; set; }
        public DateTime TransactionDate { get; set; }
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
        public string? RejectionReason { get; set; }
    }

    public class CreateIncomeTransactionRequest : CreateTransactionRequest
    {
        public CreateIncomeTransactionRequest()
        {
            Type = TransactionType.Income;
        }
        public TransactionType Type { get; set; } = TransactionType.Income;
    }

    public class CreateExpenseTransactionRequest : CreateTransactionRequest
    {
        public CreateExpenseTransactionRequest()
        {
            Type = TransactionType.Expense;
        }
        public TransactionType Type { get; set; } = TransactionType.Expense;
    }
    public class CreateTransferTransactionRequest : CreateTransactionRequest
    {
        public CreateTransferTransactionRequest()
        {
            Type = TransactionType.Transfer;
        }
        public TransactionType Type { get; set; } = TransactionType.Transfer;
    }
}
