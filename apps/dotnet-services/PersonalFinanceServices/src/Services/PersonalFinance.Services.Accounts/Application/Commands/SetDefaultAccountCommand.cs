using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.Services.Accounts.Application.Common;
using PersonalFinance.Services.Accounts.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Accounts.Application.DTOs;
using PersonalFinance.Services.Accounts.Application.Mappings;
using PersonalFinance.Services.Accounts.Infrastructure.Data;

namespace PersonalFinance.Services.Accounts.Application.Commands
{
    public class SetDefaultAccountCommand : IRequest<ApiResponse<AccountTransferObject>>
    {
        public Guid UserId { get; private set; }
        public string AccountNumber { get; private set; }
        public SetDefaultAccountCommand(Guid userId, string accountNumber)
        {
            UserId = userId;
            AccountNumber = accountNumber;
        }
    }

    public class SetDefaultAccountCommandHandler : BaseRequestHandler<SetDefaultAccountCommand, ApiResponse<AccountTransferObject>>
    {
        public SetDefaultAccountCommandHandler(AccountDbContext context, ILogger<SetDefaultAccountCommandHandler> logger, IMapper mapper)
            : base(context, logger, mapper)
        {
        }

        public async override Task<ApiResponse<AccountTransferObject>> Handle(SetDefaultAccountCommand request, CancellationToken cancellationToken)
        {


            var accounts = await Context.Accounts.Where(x => x.UserId == request.UserId).ToListAsync();
            
            if (!accounts.Any())
            {
                Logger.LogError("Account with number {AccountNumber} does not exist for user {UserId}", request.AccountNumber, request.UserId);
                return ApiResponse<AccountTransferObject>.ErrorResult("Account does not exist");
            }

            //Set default account to given account number.

            accounts.ForEach(account => account.ToggleDefault(false));
            accounts.FirstOrDefault(account => account.AccountNumber == request.AccountNumber)?.ToggleDefault(true);

            await Context.SaveChangesAsync();
            var accountTransferObject = accounts.FirstOrDefault(account => account.IsDefault == true)?.ToDto(Mapper);

            // Implementation logic for setting the default account
            // This is a placeholder as the actual logic is not provided in the original code snippet.
            return await Task.FromResult(ApiResponse<AccountTransferObject>.SuccessResult(accountTransferObject));
        }
    }
}
