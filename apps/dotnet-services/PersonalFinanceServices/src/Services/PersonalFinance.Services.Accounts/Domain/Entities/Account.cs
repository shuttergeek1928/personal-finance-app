using PersonalFinance.Services.Accounts.Domain.Events;
using PersonalFinance.Shared.Common.Domain;
using PersonalFinance.Shared.Common.Domain.ValueObjects;

namespace PersonalFinance.Services.Accounts.Domain.Entities
{
    public class Account : AggregateRoot
    {
        public string Name { get; private set; } = string.Empty;
        public AccountType Type { get; private set; }
        public Money Balance { get; private set; }
        public Guid UserId { get; private set; }
        public string AccountNumber { get; private set; } = string.Empty;
        public string? Description { get; private set; } = null;
        public bool IsDefault { get; private set; } = false;


        private Account() { }

        public Account(string name, AccountType type, Guid userId, string accountNumber, bool isDefault = false)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type;
            UserId = userId;
            AccountNumber = accountNumber;
            IsDefault = isDefault;
            Balance = new Money(0); // Initialize with zero balance

            AddDomainEvent(new AccountCreatedEvent(UserId, Name, Type));
        }

        // Business methods
        public void Deposit(Money amount)
        {
            Balance = Balance.Add(amount);
            AddDomainEvent(new BalanceUpdateEvent(UserId, Name, Type, amount));
        }

        public void Withdraw(Money amount)
        {
            if (amount.Amount > Balance.Amount)
                throw new InvalidOperationException("Insufficient funds for withdrawal.");
            Balance = Balance.Subtract(amount);
            AddDomainEvent(new BalanceUpdateEvent(UserId, Name, Type, amount, false));
        }

        public void Transfer(Account toAccount, Money amount)
        {
            if (toAccount == null)
                throw new ArgumentNullException(nameof(toAccount));

            if (amount.Amount > Balance.Amount)
                throw new InvalidOperationException("Insufficient funds for transfer.");

            Withdraw(amount);
            toAccount.Deposit(amount);
            AddDomainEvent(new BalanceUpdateEvent(UserId, Name, Type, amount, false, true));
        }

        public void ToggleDefault(bool isDefault)
        {
            IsDefault = isDefault;
            AddDomainEvent(new DefaultAccountChnagedEvent(UserId, Name, IsDefault));
        }

        public void AddDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentNullException(nameof(description), "Description cannot be null or empty.");
            Description = description;
        }
    }

    public enum AccountType
    {
        Savings,
        Checking,
        CreditCard,
        Investment
    }
}
