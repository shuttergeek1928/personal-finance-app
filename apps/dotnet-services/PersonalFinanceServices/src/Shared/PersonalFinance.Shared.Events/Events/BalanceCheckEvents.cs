using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinance.Shared.Events.Events
{
    // Request-Response pattern for balance validation
    public record CheckBalanceRequest : IIntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

        public Guid RequestId { get; init; } = Guid.NewGuid();
        public Guid AccountId { get; init; }
        public decimal Amount { get; init; }
        public string TransactionType { get; init; } = "Expense";
    }

    public record CheckBalanceResponse : IIntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

        public Guid RequestId { get; init; }
        public Guid AccountId { get; init; }
        public bool HasSufficientFunds { get; init; }
        public decimal AvailableBalance { get; init; }
        public decimal RequestedAmount { get; init; }
    }
}
