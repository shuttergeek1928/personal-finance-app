namespace PersonalFinance.Shared.Events.Events
{
    // Base event interface
    public interface IIntegrationEvent
    {
        Guid EventId { get; }
        DateTime OccurredOn { get; }
    }

    // Income transaction event
    public record IncomeTransactionCreatedEvent : IIntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

        public Guid TransactionId { get; init; }
        public Guid UserId { get; init; }
        public Guid? AccountId { get; init; }
        public Guid? CreditCardId { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = "INR";
        public string Description { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public DateTime TransactionDate { get; init; }
    }

    // Expense transaction event
    public record ExpenseTransactionCreatedEvent : IIntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

        public Guid TransactionId { get; init; }
        public Guid UserId { get; init; }
        public Guid? AccountId { get; init; }
        public Guid? CreditCardId { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = "INR";
        public string Description { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public DateTime TransactionDate { get; init; }
    }

    // Balance update event
    public record AccountBalanceUpdatedEvent : IIntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

        public Guid AccountId { get; init; }
        public Guid UserId { get; init; }
        public decimal NewBalance { get; init; }
        public decimal PreviousBalance { get; init; }
        public string Currency { get; init; } = "INR";
        public string UpdateReason { get; init; } = string.Empty;
    }

    // Transaction rejected event
    public record TransactionRejectedEvent : IIntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

        public Guid TransactionId { get; init; }
        public Guid UserId { get; init; }
        public Guid? AccountId { get; init; }
        public Guid? CreditCardId { get; init; }
        public decimal AttemptedAmount { get; init; }
        public decimal AvailableBalance { get; init; }
        public string Reason { get; init; } = string.Empty;
    }

    // Transfer transaction event
    public record TransferTransactionCreatedEvent : IIntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

        public Guid TransactionId { get; init; }
        public Guid UserId { get; init; }
        public Guid? FromAccountId { get; init; }
        public Guid? ToAccountId { get; init; }
        public Guid? FromCreditCardId { get; init; }
        public Guid? ToCreditCardId { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = "INR";
        public string Description { get; init; } = string.Empty;
        public string Category { get; init; } = "Transfer";
        public DateTime TransactionDate { get; init; }
    }

    // Batch transaction import event (from email sync / bank statement upload)
    public record EmailTransactionsBatchEvent : IIntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

        public Guid UserId { get; init; }
        public List<EmailTransactionItem> Transactions { get; init; } = new();
    }

    public record EmailTransactionItem
    {
        public decimal Amount { get; init; }
        public string Currency { get; init; } = "INR";
        public string TransactionType { get; init; } = string.Empty; // Income, Expense
        public string Category { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public DateTime TransactionDate { get; init; }
        public Guid? AccountId { get; init; }
        public string? ReferenceNumber { get; init; }
        public string Source { get; init; } = "Gmail"; // Gmail, SMS, AccountAggregator, BankStatement
    }

    // Transaction deleted event
    public record TransactionDeletedEvent : IIntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

        public Guid TransactionId { get; init; }
        public Guid UserId { get; init; }
        public Guid? AccountId { get; init; }
        public Guid? CreditCardId { get; init; }
        public decimal Amount { get; init; }
        public string TransactionType { get; init; } = string.Empty;
    }
}
