using PersonalFinance.Services.Accounts.Domain.Entities;
using PersonalFinance.Shared.Common.Domain.ValueObjects;

namespace PersonalFinance.Services.Accounts.Application.DataTransferObjects.Requests
{
    public class CreateAccountRequest
    {
        public string Name { get; set; } = string.Empty;
        public AccountType Type { get; set; }
        public Money Balance { get; set; }
        public Guid UserId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string? Description { get; set; } = null;
        public bool IsDefault { get; set; } = false;
    }
}
