using AutoMapper;
using MediatR;
using PersonalFinance.Services.Accounts.Application.Common;
using PersonalFinance.Services.Accounts.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Accounts.Application.DTOs;
using PersonalFinance.Services.Accounts.Application.Mappings;
using PersonalFinance.Services.Accounts.Application.Services;
using PersonalFinance.Services.Accounts.Domain.Entities;
using PersonalFinance.Services.Accounts.Infrastructure.Data;
using PersonalFinance.Shared.Common.Domain.ValueObjects;

namespace PersonalFinance.Services.Accounts.Application.Commands
{
    public class CreateAccountCommand : IRequest<ApiResponse<AccountTransferObject>>
    {
        public string Name { get; private set; } = string.Empty;
        public AccountType Type { get; private set; }
        public Money Balance { get; private set; }
        public Guid UserId { get; private set; }
        public string AccountNumber { get; private set; }
        public string? Description { get; private set; } = null;

        public CreateAccountCommand(string name, AccountType type, Money balance, Guid userId, string accountNumber, string? description)
        {
            Name = name;
            Type = type;
            Balance = balance;
            UserId = userId;
            AccountNumber = accountNumber;
            Description = description;
        }
    }

    public class CreateAccountCommandHandler : BaseRequestHandler<CreateAccountCommand, ApiResponse<AccountTransferObject>>
    {
        public CreateAccountCommandHandler(
            AccountDbContext context,
            IPasswordHasher passwordHasher,
            IMapper mapper,
            ILogger<CreateAccountCommandHandler> logger) : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<AccountTransferObject>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation("Starting creation for account: {Account Number}", request.AccountNumber);

                if (await AccountExistAsync(request.AccountNumber, cancellationToken))
                {
                    return ApiResponse<AccountTransferObject>.ErrorResult("An account with this account number already exists");
                }

                var account = new Account(request.Name, request.Type, request.UserId, request.AccountNumber);
                account.Deposit(request.Balance);
                account.AddDescription(request.Description);

                Context.Accounts.Add(account);
                await Context.SaveChangesAsync(cancellationToken);

                var AccountTransferObject = account.ToDto(Mapper);

                Logger.LogInformation("Account created successfully: {AccountNumber}", request.AccountNumber);

                return ApiResponse<AccountTransferObject>.SuccessResult(AccountTransferObject, "Account Created successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating account: {AccountNumber}", request.AccountNumber);
                return ApiResponse<AccountTransferObject>.ErrorResult($"An error occurred while creating the account, {ex.Message}");
            }
        }
    }
}