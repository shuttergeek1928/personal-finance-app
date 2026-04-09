namespace PersonalFinance.Services.Obligations.Application.DataTransferObjects.Requests
{
    public class MakePaymentRequest
    {
        public decimal Amount { get; set; }
        public string? Note { get; set; }
    }
}
