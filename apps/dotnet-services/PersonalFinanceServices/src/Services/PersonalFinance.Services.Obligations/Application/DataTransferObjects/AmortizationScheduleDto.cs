namespace PersonalFinance.Services.Obligations.Application.DataTransferObjects
{
    public class AmortizationScheduleItemDto
    {
        public int Month { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal EmiAmount { get; set; }
        public decimal PrincipalComponent { get; set; }
        public decimal InterestComponent { get; set; }
        public decimal OutstandingBalance { get; set; }
    }

    public class AmortizationScheduleDto
    {
        public Guid LiabilityId { get; set; }
        public string LiabilityName { get; set; } = string.Empty;
        public decimal TotalAmountPayable { get; set; }
        public decimal TotalInterestPayable { get; set; }
        public decimal MonthlyEmi { get; set; }
        public List<AmortizationScheduleItemDto> Schedule { get; set; } = new();
    }
}
