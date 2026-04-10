using PersonalFinance.Services.Transactions.Domain.Entities;
using PersonalFinance.Shared.Common.Domain;

namespace PersonalFinance.Services.Transactions.Domain.Events
{
    public class TransactionCreatedEvent : DomainEvent
    {
        public TransactionCreatedEvent(Guid id, Guid userId, Guid? accountId, Guid? creditCardId, TransactionType type, Guid? toAccountId = null, Guid? toCreditCardId = null)
        {
            Id = id;
            UserId = userId;
            AccountId = accountId;
            CreditCardId = creditCardId;
            Type = type;
            ToAccountId = toAccountId;
            ToCreditCardId = toCreditCardId;
        }

        public Guid Id { get; }
        public Guid UserId { get; }
        public Guid? AccountId { get; }
        public Guid? CreditCardId { get; }
        public TransactionType Type { get; }
        public Guid? ToAccountId { get; }
        public Guid? ToCreditCardId { get; }
    }
}