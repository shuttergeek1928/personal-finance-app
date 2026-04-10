using PersonalFinance.Services.Transactions.Domain.Events;
using PersonalFinance.Shared.Common.Domain;
using PersonalFinance.Shared.Common.Domain.ValueObjects;

namespace PersonalFinance.Services.Transactions.Domain.Entities
{
    public class Transaction : AggregateRoot
    {
        // Properties
        public Guid UserId { get; private set; }
        public Guid? AccountId { get; private set; }
        public Guid? CreditCardId { get; private set; }
        public Guid? ToAccountId { get; private set; }
        public Guid? ToCreditCardId { get; private set; }
        public Money Money { get; private set; }
        public TransactionType Type { get; private set; }
        public TransactionStatus Status { get; private set; }
        public string Description { get; private set; } = string.Empty;
        public string Category { get; private set; } = string.Empty;
        public DateTime TransactionDate { get; private set; }
        public string? RejectionReason { get; private set; }

        private Transaction() { }

        public Transaction(Guid userId, Guid? accountId, Guid? creditCardId, Money money, TransactionType type, string description, string category, DateTime transactionDate, Guid? toAccountId = null, Guid? toCreditCardId = null)
        {
            // Normalize empty GUIDs to null
            accountId = accountId == Guid.Empty ? null : accountId;
            creditCardId = creditCardId == Guid.Empty ? null : creditCardId;
            toAccountId = toAccountId == Guid.Empty ? null : toAccountId;
            toCreditCardId = toCreditCardId == Guid.Empty ? null : toCreditCardId;

            if (accountId == null && creditCardId == null)
                throw new ArgumentException("Either AccountId or CreditCardId must be provided.");

            UserId = userId;
            AccountId = accountId;
            CreditCardId = creditCardId;
            ToAccountId = toAccountId;
            ToCreditCardId = toCreditCardId;
            Money = money ?? throw new ArgumentNullException(nameof(money));
            Type = type;
            Description = description ?? throw new ArgumentNullException(nameof(description));
            Category = category ?? throw new ArgumentNullException(nameof(category));
            TransactionDate = transactionDate;
            
            // Add domain event
            AddDomainEvent(new TransactionCreatedEvent(Id, UserId, AccountId, CreditCardId, Type, ToAccountId, ToCreditCardId));
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
