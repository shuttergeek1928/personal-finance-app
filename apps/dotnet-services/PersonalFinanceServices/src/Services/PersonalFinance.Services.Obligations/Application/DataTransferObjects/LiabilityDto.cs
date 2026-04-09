using PersonalFinance.Services.Obligations.Domain.Entities;
using PersonalFinance.Shared.Common.Domain.ValueObjects;

namespace PersonalFinance.Services.Obligations.Application.DataTransferObjects
{
    public class LiabilityDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public LiabilityType Type { get; set; }
        public string LenderName { get; set; } = string.Empty;
        public Money PrincipalAmount { get; set; }
        public Money OutstandingBalance { get; set; }
        public decimal InterestRate { get; set; }
        public int TenureMonths { get; set; }
        public Money EmiAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid UserId { get; set; }
        public Guid? AccountId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
