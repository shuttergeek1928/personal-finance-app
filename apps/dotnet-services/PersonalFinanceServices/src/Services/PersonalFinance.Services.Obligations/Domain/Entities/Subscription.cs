using PersonalFinance.Shared.Common.Domain;
using PersonalFinance.Shared.Common.Domain.ValueObjects;

namespace PersonalFinance.Services.Obligations.Domain.Entities
{
    public class Subscription : AggregateRoot
    {
        public string Name { get; private set; } = string.Empty;
        public SubscriptionType Type { get; private set; }
        public string Provider { get; private set; } = string.Empty;
        public Money Amount { get; private set; }
        public BillingCycle BillingCycle { get; private set; }
        public DateTime NextBillingDate { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }
        public bool AutoRenew { get; private set; }
        public Guid UserId { get; private set; }

        private Subscription() { }

        public Subscription(
            string name,
            SubscriptionType type,
            string provider,
            decimal amount,
            BillingCycle billingCycle,
            DateTime startDate,
            Guid userId,
            bool autoRenew = true,
            DateTime? endDate = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type;
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            Amount = new Money(amount);
            BillingCycle = billingCycle;
            StartDate = startDate;
            NextBillingDate = CalculateNextBillingDate(startDate, billingCycle);
            UserId = userId;
            AutoRenew = autoRenew;
            EndDate = endDate;
        }

        /// <summary>
        /// Advances NextBillingDate to the next cycle.
        /// </summary>
        public void Renew()
        {
            if (!AutoRenew)
                throw new InvalidOperationException("Subscription is not set to auto-renew.");

            NextBillingDate = CalculateNextBillingDate(NextBillingDate, BillingCycle);
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(
            string name,
            SubscriptionType type,
            string provider,
            decimal amount,
            BillingCycle billingCycle,
            DateTime startDate,
            bool autoRenew,
            DateTime? endDate)
        {
            Name = name;
            Type = type;
            Provider = provider;
            Amount = new Money(amount);
            BillingCycle = billingCycle;
            StartDate = startDate;
            AutoRenew = autoRenew;
            EndDate = endDate;
            NextBillingDate = CalculateNextBillingDate(startDate, billingCycle);
            UpdatedAt = DateTime.UtcNow;
        }

        private static DateTime CalculateNextBillingDate(DateTime fromDate, BillingCycle cycle) => cycle switch
        {
            BillingCycle.Monthly => fromDate.AddMonths(1),
            BillingCycle.Quarterly => fromDate.AddMonths(3),
            BillingCycle.HalfYearly => fromDate.AddMonths(6),
            BillingCycle.Yearly => fromDate.AddYears(1),
            _ => fromDate.AddMonths(1)
        };
    }

    public enum SubscriptionType
    {
        Entertainment,
        Utility,
        Insurance,
        Software,
        Fitness,
        Other
    }

    public enum BillingCycle
    {
        Monthly,
        Quarterly,
        HalfYearly,
        Yearly
    }
}
