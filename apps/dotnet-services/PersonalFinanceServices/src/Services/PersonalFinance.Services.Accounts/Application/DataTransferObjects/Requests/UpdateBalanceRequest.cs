using PersonalFinance.Shared.Common.Domain.ValueObjects;

namespace PersonalFinance.Services.Accounts.Application.DataTransferObjects.Requests
{
    public class UpdateBalanceRequest
    {
        public Guid Id { get; set; }
        public Money Balance { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public bool IsDeposit { get; set; } = true; // Default to deposit operation
    }

    public class TransferMoneyRequest : UpdateBalanceRequest
    {
        public Guid ToAccountId { get; set; }
    }
}
