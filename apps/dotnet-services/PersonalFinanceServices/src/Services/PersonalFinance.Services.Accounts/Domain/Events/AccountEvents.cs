using PersonalFinance.Services.Accounts.Domain.Entities;
using PersonalFinance.Shared.Common.Domain;
using PersonalFinance.Shared.Common.Domain.ValueObjects;

namespace PersonalFinance.Services.Accounts.Domain.Events
{
    public class AccountCreatedEvent : DomainEvent
    {
        public AccountCreatedEvent(Guid userId, string name, AccountType type)
        {
            UserId = userId;
            Name = name;
            AccountType = type;
        }

        public Guid UserId { get; }
        public string Name { get; }
        public AccountType AccountType { get; }
    }

    public class BalanceUpdateEvent : DomainEvent
    {
        public BalanceUpdateEvent(Guid userId, string name, AccountType type, Money money, bool isDeposit = true, bool isTransfer = false)
        {
            UserId = userId;
            Name = name;
            AccountType = type;
            Amount = money ?? throw new ArgumentNullException(nameof(money), "Money cannot be null");
            IsDeposit = isDeposit;
            IsTransfer = isTransfer;
        }

        public Guid UserId { get; }
        public string Name { get; }
        public AccountType AccountType { get; }
        public Money Amount { get; }
        public bool IsDeposit { get; set; } = false;
        public bool IsTransfer { get; set; } = false;
    }

    public class DefaultAccountChnagedEvent : DomainEvent
    {
        public DefaultAccountChnagedEvent(Guid userId, string name, bool isDefault)
        {
            UserId = userId;
            Name = name ?? throw new ArgumentNullException(nameof(name), "Name cannot be null");
            IsDefault = isDefault;
        }

        public Guid UserId { get; }
        public string Name { get; }
        public bool IsDefault { get; }
    }
}