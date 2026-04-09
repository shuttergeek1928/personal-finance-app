using PersonalFinance.Services.Obligations.Domain.Entities;
using PersonalFinance.Shared.Common.Domain.ValueObjects;

namespace PersonalFinance.Services.Obligations.Application.DataTransferObjects.Requests
{
    public class CreateLiabilityRequest
    {
        public string Name { get; set; } = string.Empty;
        public LiabilityType Type { get; set; }
        public string LenderName { get; set; } = string.Empty;
        public decimal PrincipalAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int TenureMonths { get; set; }
        public DateTime StartDate { get; set; }
        public Guid UserId { get; set; }
        public Guid? AccountId { get; set; }
    }
}
