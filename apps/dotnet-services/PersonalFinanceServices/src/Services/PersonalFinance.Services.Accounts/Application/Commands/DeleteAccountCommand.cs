using AutoMapper;

using MediatR;

using PersonalFinance.Services.Accounts.Application.Common;
using PersonalFinance.Services.Accounts.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Accounts.Application.DTOs;
using PersonalFinance.Services.Accounts.Application.Mappings;
using PersonalFinance.Services.Accounts.Infrastructure.Data;

namespace PersonalFinance.Services.Accounts.Application.Commands
{
    public class DeleteAccountCommand : IRequest<ApiResponse<AccountTransferObject>>
    {
        public Guid AccountId { get; set; }
        public Guid UserId { get; set; }

        public DeleteAccountCommand(Guid accountId, Guid userId)
        {
            AccountId = accountId;
            UserId = userId;
        }
    }

    public class DeleteAccountCommandHandler : BaseRequestHandler<DeleteAccountCommand, ApiResponse<AccountTransferObject>>
    {
        public DeleteAccountCommandHandler(AccountDbContext context, ILogger<DeleteAccountCommandHandler> logger, IMapper mapper)
            : base(context, logger, mapper)
        {
        }

        public async override Task<ApiResponse<AccountTransferObject>> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
        {

            var account = await Context.Accounts.FindAsync(request.AccountId);

            if (account is null && !account.IsActive)
            {
                Logger.LogError("Account with account id {AccountId} does not exist for user {UserId}", request.AccountId, request.UserId);
                return ApiResponse<AccountTransferObject>.ErrorResult("Account does not exist");
            }

            //Mark the account as inactive instead of deleting it from the database

            account.TogggleActiveStatus(false);

            await Context.SaveChangesAsync();
            var accountTransferObject = account?.ToDto(Mapper);

            // Implementation logic for setting the default account
            // This is a placeholder as the actual logic is not provided in the original code snippet.
            return await Task.FromResult(ApiResponse<AccountTransferObject>.SuccessResult(accountTransferObject));
        }
    }
}
