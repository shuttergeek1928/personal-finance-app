﻿using AutoMapper;
using MediatR;
using PersonalFinance.Services.Accounts.Application.Common;
using PersonalFinance.Services.Accounts.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Accounts.Infrastructure.Data;
using PersonalFinance.Shared.Common.Domain.ValueObjects;

namespace PersonalFinance.Services.Accounts.Application.Commands
{
    public class TransferFundCommand : IRequest<ApiResponse<bool>>
    {
        public Guid Id { get; set; }
        public Money Money { get; set; } = new Money(0, "INR"); // Default to 0 INR
        public Guid ToAccountId { get; set; }

        public TransferFundCommand(Guid id, Money money, Guid toAccountId)
        {
            Id = id;
            Money = money;
            ToAccountId = toAccountId;
        }
    }

    public class TransferFundsHandler : BaseRequestHandler<TransferFundCommand, ApiResponse<bool>>
    {
        public TransferFundsHandler(AccountDbContext context, ILogger<TransferFundsHandler> logger, IMapper mapper) : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<bool>> Handle(TransferFundCommand request, CancellationToken token)
        {
            var account = await Context.Accounts.FindAsync(request.Id);

            if (account == null)
            {
                Logger.LogError("Account with ID {ID} not found", request.Id);
                return ApiResponse<bool>.ErrorResult("Account not found");
            }

            var toAccount = await Context.Accounts.FindAsync(request.ToAccountId);

            if (toAccount == null)
            {
                Logger.LogError("To Account with ID {ID} not found", request.ToAccountId);
                return ApiResponse<bool>.ErrorResult("To Account not found");
            }

            account.Transfer(toAccount, request.Money);

            await Context.SaveChangesAsync();

            Logger.LogInformation("Account balance transferred for account id : {Id} from account id {ToId}", request.Id, request.ToAccountId);
            return ApiResponse<bool>.SuccessResult(true, "Account balance transferred successfully");
        }
    }
}
