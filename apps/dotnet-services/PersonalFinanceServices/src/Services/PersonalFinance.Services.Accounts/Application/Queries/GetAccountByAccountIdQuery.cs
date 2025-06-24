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
    public class GetAccountByAccountIdQuery : IRequest<ApiResponse<AccountTransferObject>>
    {
        public Guid Id { get; set; }

        public GetAccountByAccountIdQuery(Guid id)
        {
            Id = id;
        }
    }

    public class GetAccountByAccountIdQueryHandler : BaseQueryHandler<GetAccountByAccountIdQuery, ApiResponse<AccountTransferObject>>
    {
        public GetAccountByAccountIdQueryHandler(AccountDbContext context, ILogger<GetAccountByAccountIdQueryHandler> logger, IMapper mapper)
            : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<AccountTransferObject>> Handle(GetAccountByAccountIdQuery request, CancellationToken cancellationToken)
        {
            var account = await Context.Accounts
                .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (account == null)
            {
                Logger.LogError("Account with Id {Id} not found", request.Id);
                return ApiResponse<AccountTransferObject>.ErrorResult("Account not found");
            }

            var AccountTransferObject = account.ToDto(Mapper);
            return ApiResponse<AccountTransferObject>.SuccessResult(AccountTransferObject);
        }
    }
}