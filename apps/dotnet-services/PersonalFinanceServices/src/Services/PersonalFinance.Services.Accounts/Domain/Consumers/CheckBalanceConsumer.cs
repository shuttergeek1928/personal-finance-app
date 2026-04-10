using MassTransit;
using PersonalFinance.Services.Accounts.Infrastructure.Data;
using PersonalFinance.Shared.Events.Events;

namespace PersonalFinance.Services.Accounts.Domain.Consumers
{
    public class CheckBalanceConsumer : IConsumer<CheckBalanceRequest>
    {
        private readonly ILogger<CheckBalanceConsumer> _logger;
        private readonly AccountDbContext _accountDbContext;

        public CheckBalanceConsumer(ILogger<CheckBalanceConsumer> logger, AccountDbContext accountDbContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accountDbContext = accountDbContext ?? throw new ArgumentNullException(nameof(accountDbContext));
        }

        public async Task Consume(ConsumeContext<CheckBalanceRequest> context)
        {
            var request = context.Message;
            _logger.LogInformation("Processing CheckBalanceRequest {RequestId} for Account {AccountId}, Amount {Amount}", 
                request.RequestId, request.AccountId, request.Amount);

            var account = await _accountDbContext.Accounts.FindAsync(new object[] { request.AccountId }, context.CancellationToken);

            if (account == null)
            {
                _logger.LogWarning("Account {AccountId} not found for CheckBalanceRequest {RequestId}", request.AccountId, request.RequestId);
                // Return false if account is not found (fail safe)
                await context.RespondAsync(new CheckBalanceResponse
                {
                    RequestId = request.RequestId,
                    AccountId = request.AccountId,
                    HasSufficientFunds = false,
                    AvailableBalance = 0,
                    RequestedAmount = request.Amount
                });
                return;
            }

            bool hasSufficientFunds = true;

            // Only check balance if it's an expense or transfer (Income doesn't deduct balance)
            // Wait, Expense or Transfer deduction, CheckBalance is generally called when deductions apply
            if (account.Balance.Amount < request.Amount)
            {
                hasSufficientFunds = false;
                _logger.LogWarning("Insufficient funds for Account {AccountId}. Requested: {Amount}, Available: {Available}", 
                    request.AccountId, request.Amount, account.Balance.Amount);
            }

            await context.RespondAsync(new CheckBalanceResponse
            {
                RequestId = request.RequestId,
                AccountId = request.AccountId,
                HasSufficientFunds = hasSufficientFunds,
                AvailableBalance = account.Balance.Amount,
                RequestedAmount = request.Amount
            });
        }
    }
}
