using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.Services.Obligations.Application.Common;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Obligations.Infrastructure.Data;

namespace PersonalFinance.Services.Obligations.Application.Commands
{
    public class DeleteCreditCardCommand : IRequest<ApiResponse<bool>>
    {
        public Guid Id { get; set; }
        public DeleteCreditCardCommand(Guid id)
        {
            Id = id;
        }
    }

    public class DeleteCreditCardCommandHandler : BaseRequestHandler<DeleteCreditCardCommand, ApiResponse<bool>>
    {
        public DeleteCreditCardCommandHandler(
            ObligationDbContext context,
            ILogger<DeleteCreditCardCommandHandler> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<bool>> Handle(DeleteCreditCardCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var creditCard = await Context.CreditCards.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
                if (creditCard == null)
                    return ApiResponse<bool>.ErrorResult("Credit Card not found");

                Context.CreditCards.Remove(creditCard);
                await Context.SaveChangesAsync(cancellationToken);

                return ApiResponse<bool>.SuccessResult(true, "Credit Card deleted successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error deleting credit card {Id}", request.Id);
                return ApiResponse<bool>.ErrorResult($"An error occurred while deleting credit card: {ex.Message}");
            }
        }
    }
}
