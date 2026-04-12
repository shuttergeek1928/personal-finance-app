using AutoMapper;

using MediatR;

using Microsoft.EntityFrameworkCore;

using PersonalFinance.Services.Obligations.Application.Common;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Obligations.Domain.Entities;
using PersonalFinance.Services.Obligations.Infrastructure.Data;
using PersonalFinance.Shared.Common.Domain.ValueObjects;

namespace PersonalFinance.Services.Obligations.Application.Commands
{
    public class UpdateCreditCardCommand : IRequest<ApiResponse<CreditCardDto>>
    {
        public Guid Id { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string CardName { get; set; } = string.Empty;
        public string Last4Digits { get; set; } = string.Empty;
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public CreditCardNetwork NetworkProvider { get; set; }
        public decimal TotalLimit { get; set; }
        public decimal OutstandingAmount { get; set; }
    }

    public class UpdateCreditCardCommandHandler : BaseRequestHandler<UpdateCreditCardCommand, ApiResponse<CreditCardDto>>
    {
        public UpdateCreditCardCommandHandler(
            ObligationDbContext context,
            ILogger<UpdateCreditCardCommandHandler> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<CreditCardDto>> Handle(UpdateCreditCardCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var creditCard = await Context.CreditCards.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
                if (creditCard == null)
                    return ApiResponse<CreditCardDto>.ErrorResult("Credit Card not found");

                creditCard.Update(
                    request.BankName,
                    request.CardName,
                    request.Last4Digits,
                    request.ExpiryMonth,
                    request.ExpiryYear,
                    request.NetworkProvider,
                    new Money(request.TotalLimit, "INR"),
                    new Money(request.OutstandingAmount, "INR")
                );

                await Context.SaveChangesAsync(cancellationToken);

                var dto = Mapper.Map<CreditCardDto>(creditCard);
                return ApiResponse<CreditCardDto>.SuccessResult(dto, "Credit Card updated successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating credit card {Id}", request.Id);
                return ApiResponse<CreditCardDto>.ErrorResult($"An error occurred while updating credit card: {ex.Message}");
            }
        }
    }
}
