using PersonalFinance.Services.Transactions.Domain.Entities;
using PersonalFinance.Shared.Common.Domain;

namespace PersonalFinance.Services.Transactions.Domain.Events
{
    public class TransactionCreatedEvent : DomainEvent
    {
        public TransactionCreatedEvent(Guid id, Guid userId, Guid accountId, TransactionType type)
        {
            Id = id;
            UserId = userId;
            AccountId = accountId;
            Type = type;
        }

        public Guid Id { get; }
        public Guid UserId { get; }
        public Guid AccountId { get; }
        public TransactionType Type { get; }
    }
}