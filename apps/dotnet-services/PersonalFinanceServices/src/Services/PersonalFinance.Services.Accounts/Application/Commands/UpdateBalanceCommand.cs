using AutoMapper;
using MediatR;
using PersonalFinance.Services.Accounts.Application.Common;
using PersonalFinance.Services.Accounts.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Accounts.Infrastructure.Data;
using PersonalFinance.Shared.Common.Domain.ValueObjects;

namespace PersonalFinance.Services.Accounts.Application.Commands
{
    public class UpdateBalanceCommand : IRequest<ApiResponse<bool>>
    {
        public Guid Id { get; set; }
        public Money Money { get; set; } = new Money(0, "INR"); // Default to 0 INR
        public bool IsDeposit { get; set; } = true; // Default to deposit operation

        public UpdateBalanceCommand(Guid id, Money money, bool isDeposit)
        {
            Id = id;
            Money = money;
            IsDeposit = isDeposit;
        }
    }

    public class UpdateBalanceHandler : BaseRequestHandler<UpdateBalanceCommand, ApiResponse<bool>>
    {
        public UpdateBalanceHandler(AccountDbContext context, ILogger<UpdateBalanceHandler> logger, IMapper mapper) : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<bool>> Handle(UpdateBalanceCommand request, CancellationToken token)
        {
            var account = await Context.Accounts.FindAsync(request.Id);

            if (account == null)
            {
                Logger.LogError("Account with ID {ID} not found", request.Id);
                return ApiResponse<bool>.ErrorResult("Account not found");
            }

            if (request.IsDeposit)
                account.Deposit(request.Money);
            else
                account.Withdraw(request.Money);

            await Context.SaveChangesAsync();

            Logger.LogInformation("Account balance for account id: {Id}", request.Id);
            return ApiResponse<bool>.SuccessResult(true, "Account balance updated successfully");
        }
    }
}
