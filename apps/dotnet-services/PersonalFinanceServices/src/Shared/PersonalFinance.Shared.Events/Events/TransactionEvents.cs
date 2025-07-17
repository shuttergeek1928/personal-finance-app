using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public Guid AccountId { get; init; }
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
        public Guid AccountId { get; init; }
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
        public Guid AccountId { get; init; }
        public decimal AttemptedAmount { get; init; }
        public decimal AvailableBalance { get; init; }
        public string Reason { get; init; } = string.Empty;
    }
}
