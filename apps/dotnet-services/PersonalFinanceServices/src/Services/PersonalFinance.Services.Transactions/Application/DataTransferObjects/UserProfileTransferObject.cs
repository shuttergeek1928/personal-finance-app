using PersonalFinance.Services.Transactions.Application.Extensions;

namespace PersonalFinance.Services.Transactions.Application.DataTransferObjects
{
    public class UserProfileTransferObject
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Currency { get; set; } = "USD";
        public string TimeZone { get; set; } = "UTC";
        public string Language { get; set; } = "en-US";
        public string? Avatar { get; set; }
        public string? FinancialGoals { get; set; }

        // Computed properties
        public int? Age => DateOfBirth?.CalculateAge();
    }
}
