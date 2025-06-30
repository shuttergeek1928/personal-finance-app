using PersonalFinance.Services.Transactions.Application.DataTransferObjects;

namespace PersonalFinance.Services.Transactions.Application.DTOs
{
    public class UserTransferObject
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }

        // Computed properties for convenience
        public string FullName => $"{FirstName} {LastName}";

        // Related data
        public UserProfileTransferObject? Profile { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
