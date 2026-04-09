using PersonalFinance.Services.Obligations.Domain.Entities;

namespace PersonalFinance.Services.Obligations.Application.DataTransferObjects.Requests
{
    public class UpdateCreditCardRequest
    {
        public string BankName { get; set; } = string.Empty;
        public string CardName { get; set; } = string.Empty;
        public string Last4Digits { get; set; } = string.Empty;
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public CreditCardNetwork NetworkProvider { get; set; }
        public decimal TotalLimit { get; set; }
        public decimal OutstandingAmount { get; set; }
    }
}
