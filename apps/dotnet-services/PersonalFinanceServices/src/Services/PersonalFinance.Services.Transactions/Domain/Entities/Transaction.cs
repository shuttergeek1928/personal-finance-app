using PersonalFinance.Services.Transactions.Domain.Events;
using PersonalFinance.Shared.Common.Domain;
using PersonalFinance.Shared.Common.Domain.ValueObjects;

namespace PersonalFinance.Services.Transactions.Domain.Entities
{
    public class Transaction : AggregateRoot
    {
        // Properties
        public Guid UserId { get; private set; }
        public Guid AccountId { get; private set; }
        public Money Money { get; private set; }
        public TransactionType Type { get; private set; }
        public string Description { get; private set; } = string.Empty;
        public string Category { get; private set; } = string.Empty;
        public DateTime TransactionDate { get; private set; }
        public TransactionStatus Status { get; private set; } = TransactionStatus.Pending;
        public string? RejectionReason { get; private set; }

        private Transaction() { }

        public Transaction(Guid userId, Guid accountId, Money money, TransactionType type, string description, string category, DateTime transactionDate)
        {
            UserId = userId;
            AccountId = accountId;
            Money = money ?? throw new ArgumentNullException(nameof(money));
            Type = type;
            Description = description ?? throw new ArgumentNullException(nameof(description));
            Category = category ?? throw new ArgumentNullException(nameof(category));
            TransactionDate = transactionDate;
            // Add domain event
            AddDomainEvent(new TransactionCreatedEvent(Id, UserId, AccountId, Type));
        }

        // Business methods
        public void Approve()
        {
            Status = TransactionStatus.Approved;
        }
        public void Reject(string reason)
        {
            Status = TransactionStatus.Rejected;
            RejectionReason = reason ?? throw new ArgumentNullException(nameof(reason));
        }
    }

    public enum TransactionType
    {
        Income,
        Expense,
        Transfer
    }
    public enum TransactionStatus
    {
        Pending,
        Approved,
        Rejected
    }
}
