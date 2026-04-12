using AutoMapper;

using MediatR;

using PersonalFinance.Services.Obligations.Application.Common;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Obligations.Domain.Entities;
using PersonalFinance.Services.Obligations.Infrastructure.Data;
using PersonalFinance.Shared.Common.Domain.ValueObjects;

namespace PersonalFinance.Services.Obligations.Application.Commands
{
    public class AddCreditCardCommand : IRequest<ApiResponse<CreditCardDto>>
    {
        public Guid UserId { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string CardName { get; set; } = string.Empty;
        public string Last4Digits { get; set; } = string.Empty;
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public CreditCardNetwork NetworkProvider { get; set; }
        public decimal TotalLimit { get; set; }
        public decimal OutstandingAmount { get; set; }
    }

    public class AddCreditCardCommandHandler : BaseRequestHandler<AddCreditCardCommand, ApiResponse<CreditCardDto>>
    {
        public AddCreditCardCommandHandler(
            ObligationDbContext context,
            ILogger<AddCreditCardCommandHandler> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<CreditCardDto>> Handle(AddCreditCardCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var creditCard = new CreditCard(
                    request.UserId,
                    request.BankName,
                    request.CardName,
                    request.Last4Digits,
                    request.ExpiryMonth,
                    request.ExpiryYear,
                    request.NetworkProvider,
                    new Money(request.TotalLimit, "INR"),
                    new Money(request.OutstandingAmount, "INR")
                );

                Context.CreditCards.Add(creditCard);
                await Context.SaveChangesAsync(cancellationToken);

                var dto = Mapper.Map<CreditCardDto>(creditCard);
                return ApiResponse<CreditCardDto>.SuccessResult(dto, "Credit Card added successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error adding credit card for user {UserId}", request.UserId);
                return ApiResponse<CreditCardDto>.ErrorResult($"An error occurred while adding credit card: {ex.Message}");
            }
        }
    }
}
