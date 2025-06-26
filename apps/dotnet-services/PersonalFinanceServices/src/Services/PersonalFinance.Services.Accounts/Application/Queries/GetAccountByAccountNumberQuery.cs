using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.Services.Accounts.Application.Common;
using PersonalFinance.Services.Accounts.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Accounts.Application.DTOs;
using PersonalFinance.Services.Accounts.Application.Mappings;
using PersonalFinance.Services.Accounts.Infrastructure.Data;

namespace PersonalFinance.Services.Accounts.Application.Queries
{
    public class GetAccountByAccountNumberQuery : IRequest<ApiResponse<AccountTransferObject>>
    {
        public string AccountNumber { get; set; }

        public GetAccountByAccountNumberQuery(string accountNumber)
        {
            AccountNumber = accountNumber;
        }
    }

    public class GetAccountByAccountNumberQueryHandler : BaseQueryHandler<GetAccountByAccountNumberQuery, ApiResponse<AccountTransferObject>>
    {
        public GetAccountByAccountNumberQueryHandler(AccountDbContext context, ILogger<GetAccountByAccountNumberQueryHandler> logger, IMapper mapper)
            : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<AccountTransferObject>> Handle(GetAccountByAccountNumberQuery request, CancellationToken cancellationToken)
        {
            var account = await Context.Accounts
                .FirstOrDefaultAsync(a => a.AccountNumber == request.AccountNumber, cancellationToken);

            if (account == null)
            {
                Logger.LogError("Account with {AccountNumber} not found", request.AccountNumber);
                return ApiResponse<AccountTransferObject>.ErrorResult("Account not found");
            }

            var AccountTransferObject = account.ToDto(Mapper);
            return ApiResponse<AccountTransferObject>.SuccessResult(AccountTransferObject);
        }
    }
}