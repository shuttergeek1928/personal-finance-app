using PersonalFinance.Services.Transactions.Domain.Entities;
using PersonalFinance.Shared.Common.Domain;

namespace PersonalFinance.Services.Transactions.Domain.Events
{
    public class TransactionCreatedEvent : DomainEvent
    {
        public TransactionCreatedEvent(Guid id, Guid userId, Guid accountId, TransactionType type, Guid? toAccountId = null)
        {
            Id = id;
            UserId = userId;
            AccountId = accountId;
            Type = type;
            ToAccountId = toAccountId;
        }

        public Guid Id { get; }
        public Guid UserId { get; }
        public Guid AccountId { get; }
        public TransactionType Type { get; }
        public Guid? ToAccountId { get; }
    }
}