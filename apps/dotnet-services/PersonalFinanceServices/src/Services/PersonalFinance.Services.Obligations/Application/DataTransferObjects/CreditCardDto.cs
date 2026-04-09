using PersonalFinance.Services.Obligations.Domain.Entities;
using PersonalFinance.Shared.Common.Domain.ValueObjects;

namespace PersonalFinance.Services.Obligations.Application.DataTransferObjects
{
    public class CreditCardDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string CardName { get; set; } = string.Empty;
        public string Last4Digits { get; set; } = string.Empty;
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public CreditCardNetwork NetworkProvider { get; set; }
        public Money TotalLimit { get; set; } = default!;
        public Money OutstandingAmount { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
