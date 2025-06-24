using PersonalFinance.Services.Accounts.Domain.Entities;
using PersonalFinance.Shared.Common.Domain.ValueObjects;

namespace PersonalFinance.Services.Accounts.Application.DTOs
{
    public class AccountTransferObject
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public AccountType Type { get; set; }
        public Money Balance { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
    }
}
