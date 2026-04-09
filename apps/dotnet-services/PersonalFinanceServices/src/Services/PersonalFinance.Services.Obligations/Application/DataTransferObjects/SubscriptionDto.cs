using PersonalFinance.Services.Obligations.Domain.Entities;
using PersonalFinance.Shared.Common.Domain.ValueObjects;

namespace PersonalFinance.Services.Obligations.Application.DataTransferObjects
{
    public class SubscriptionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public SubscriptionType Type { get; set; }
        public string Provider { get; set; } = string.Empty;
        public Money Amount { get; set; }
        public BillingCycle BillingCycle { get; set; }
        public DateTime NextBillingDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool AutoRenew { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
