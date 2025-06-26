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
    public class GetAccountByUserIdQuery : IRequest<ApiResponse<List<AccountTransferObject>>>
    {
        public Guid UserId { get; set; }

        public GetAccountByUserIdQuery(Guid userId)
        {
            UserId = userId;
        }
    }

    public class GetAccountByUserIdQueryQueryHandler : BaseQueryHandler<GetAccountByUserIdQuery, ApiResponse<List<AccountTransferObject>>>
    {
        public GetAccountByUserIdQueryQueryHandler(AccountDbContext context, ILogger<GetAccountByUserIdQueryQueryHandler> logger, IMapper mapper)
            : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<List<AccountTransferObject>>> Handle(GetAccountByUserIdQuery request, CancellationToken cancellationToken)
        {
            var accounts = await Context.Accounts.Where(a => a.UserId == request.UserId).ToListAsync();

            if (!accounts.Any())
            {
                Logger.LogError("Accounts with Id {Id} not found", request.UserId);
                return ApiResponse<List<AccountTransferObject>>.ErrorResult("0 account found");
            }

            var accountTransferObjects = accounts.Select(account => account.ToDto(Mapper)).ToList();
            return ApiResponse<List<AccountTransferObject>>.SuccessResult(accountTransferObjects);
        }
    }
}