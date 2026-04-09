using PersonalFinance.Shared.Common.Domain;
using PersonalFinance.Shared.Common.Domain.ValueObjects;

namespace PersonalFinance.Services.Obligations.Domain.Entities
{
    public class CreditCard : AggregateRoot
    {
        public Guid UserId { get; private set; }
        public string BankName { get; private set; } = string.Empty;
        public string CardName { get; private set; } = string.Empty;
        public string Last4Digits { get; private set; } = string.Empty;
        public int ExpiryMonth { get; private set; }
        public int ExpiryYear { get; private set; }
        public CreditCardNetwork NetworkProvider { get; private set; }
        public Money TotalLimit { get; private set; } = default!;
        public Money OutstandingAmount { get; private set; } = default!;

        private CreditCard() { }

        public CreditCard(Guid userId, string bankName, string cardName, string last4Digits, int expiryMonth, int expiryYear, CreditCardNetwork networkProvider, Money totalLimit, Money outstandingAmount)
        {
            if (string.IsNullOrWhiteSpace(bankName))
                throw new ArgumentException("Bank name cannot be empty.", nameof(bankName));
            if (string.IsNullOrWhiteSpace(last4Digits) || last4Digits.Length != 4)
                throw new ArgumentException("Must provide exactly the last 4 digits of the card.", nameof(last4Digits));
            if (expiryMonth < 1 || expiryMonth > 12)
                throw new ArgumentException("Expiry month must be between 1 and 12.", nameof(expiryMonth));
            
            UserId = userId;
            BankName = bankName;
            CardName = cardName ?? string.Empty;
            Last4Digits = last4Digits;
            ExpiryMonth = expiryMonth;
            ExpiryYear = expiryYear;
            NetworkProvider = networkProvider;
            TotalLimit = totalLimit ?? throw new ArgumentNullException(nameof(totalLimit));
            OutstandingAmount = outstandingAmount ?? throw new ArgumentNullException(nameof(outstandingAmount));
        }

        public void Update(string bankName, string cardName, string last4Digits, int expiryMonth, int expiryYear, CreditCardNetwork networkProvider, Money totalLimit, Money outstandingAmount)
        {
            if (string.IsNullOrWhiteSpace(bankName))
                throw new ArgumentException("Bank name cannot be empty.", nameof(bankName));
            if (string.IsNullOrWhiteSpace(last4Digits) || last4Digits.Length != 4)
                throw new ArgumentException("Must provide exactly the last 4 digits of the card.", nameof(last4Digits));
            if (expiryMonth < 1 || expiryMonth > 12)
                throw new ArgumentException("Expiry month must be between 1 and 12.", nameof(expiryMonth));

            BankName = bankName;
            CardName = cardName ?? string.Empty;
            Last4Digits = last4Digits;
            ExpiryMonth = expiryMonth;
            ExpiryYear = expiryYear;
            NetworkProvider = networkProvider;
            TotalLimit = totalLimit ?? throw new ArgumentNullException(nameof(totalLimit));
            OutstandingAmount = outstandingAmount ?? throw new ArgumentNullException(nameof(outstandingAmount));

            UpdatedAt = DateTime.UtcNow;
        }
    }
}
