using PersonalFinance.Services.UserManagement.Domain.Entities;
using PersonalFinance.Shared.Common.Domain;

namespace PersonalFinance.Services.UserManagement.Domain.Entities
{

    public class UserProfile : UserOwnedEntity
    {
        private UserProfile() { } // EF Core

        public UserProfile(Guid userId, DateTime? dateOfBirth = null, string currency = "USD")
        {
            UserId = userId;
            DateOfBirth = dateOfBirth;
            Currency = currency;
            TimeZone = "UTC";
            Language = "en-US";
        }

        public DateTime? DateOfBirth { get; private set; }
        public string Currency { get; private set; } = "INR";
        public string TimeZone { get; private set; } = "UTC";
        public string Language { get; private set; } = "en-US";
        public string? Avatar { get; private set; }
        public string? FinancialGoals { get; private set; } // JSON

        // Navigation property
        public User User { get; private set; } = null!;

        public void UpdatePreferences(string currency, string timeZone, string language)
        {
            Currency = currency ?? throw new ArgumentNullException(nameof(currency));
            TimeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
            Language = language ?? throw new ArgumentNullException(nameof(language));
        }

        public void SetAvatar(string avatarUrl)
        {
            Avatar = avatarUrl;
        }

        public void UpdateFinancialGoals(string goalsJson)
        {
            FinancialGoals = goalsJson;
        }
    }
}
