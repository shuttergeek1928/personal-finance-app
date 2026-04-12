namespace PersonalFinance.Services.Obligations.Application.DataTransferObjects
{
    public class ObligationDashboardDto
    {
        public int TotalActiveLiabilities { get; set; }
        public decimal TotalOutstandingBalance { get; set; }
        public decimal TotalMonthlyEmi { get; set; }
        public int TotalActiveSubscriptions { get; set; }
        public decimal TotalMonthlySubscriptionCost { get; set; }
        public decimal TotalMonthlyObligations { get; set; }
        public List<LiabilityDto> Liabilities { get; set; } = new();
        public List<SubscriptionDto> Subscriptions { get; set; } = new();
    }
}
