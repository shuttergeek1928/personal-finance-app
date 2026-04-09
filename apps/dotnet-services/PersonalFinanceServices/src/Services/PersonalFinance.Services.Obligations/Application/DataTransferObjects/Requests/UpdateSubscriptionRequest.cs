using PersonalFinance.Services.Obligations.Domain.Entities;

namespace PersonalFinance.Services.Obligations.Application.DataTransferObjects.Requests
{
    public class UpdateSubscriptionRequest
    {
        public string Name { get; set; } = string.Empty;
        public SubscriptionType Type { get; set; }
        public string Provider { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public BillingCycle BillingCycle { get; set; }
        public DateTime StartDate { get; set; }
        public bool AutoRenew { get; set; } = true;
        public DateTime? EndDate { get; set; }
    }
}
