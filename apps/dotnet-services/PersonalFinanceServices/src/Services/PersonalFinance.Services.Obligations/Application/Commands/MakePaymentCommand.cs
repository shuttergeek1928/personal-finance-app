using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.Services.Obligations.Application.Common;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Obligations.Application.Mappings;
using PersonalFinance.Services.Obligations.Infrastructure.Data;
using PersonalFinance.Shared.Common.Domain.ValueObjects;

namespace PersonalFinance.Services.Obligations.Application.Commands
{
    public class MakePaymentCommand : IRequest<ApiResponse<LiabilityDto>>
    {
        public Guid LiabilityId { get; set; }
        public decimal Amount { get; set; }
        public string? Note { get; set; }

        public MakePaymentCommand(Guid liabilityId, decimal amount, string? note)
        {
            LiabilityId = liabilityId;
            Amount = amount;
            Note = note;
        }
    }

    public class MakePaymentCommandHandler : BaseRequestHandler<MakePaymentCommand, ApiResponse<LiabilityDto>>
    {
        public MakePaymentCommandHandler(
            ObligationDbContext context,
            ILogger<MakePaymentCommandHandler> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<LiabilityDto>> Handle(MakePaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var liability = await Context.Liabilities
                    .FirstOrDefaultAsync(l => l.Id == request.LiabilityId && l.IsActive, cancellationToken);

                if (liability == null)
                {
                    Logger.LogWarning("Liability not found: {Id}", request.LiabilityId);
                    return ApiResponse<LiabilityDto>.ErrorResult("Liability not found");
                }

                if (request.Amount <= 0)
                {
                    return ApiResponse<LiabilityDto>.ErrorResult("Payment amount must be positive");
                }

                if (request.Amount > liability.OutstandingBalance.Amount)
                {
                    return ApiResponse<LiabilityDto>.ErrorResult(
                        $"Payment amount (₹{request.Amount:N2}) exceeds outstanding balance (₹{liability.OutstandingBalance.Amount:N2})");
                }

                liability.MakePayment(new Money(request.Amount));

                await Context.SaveChangesAsync(cancellationToken);

                var dto = liability.ToDto(Mapper);

                Logger.LogInformation(
                    "Payment of {Amount} recorded for liability {Id}. Outstanding: {Outstanding}",
                    request.Amount, liability.Id, liability.OutstandingBalance.Amount);

                return ApiResponse<LiabilityDto>.SuccessResult(dto,
                    $"Payment of ₹{request.Amount:N2} recorded successfully. Outstanding balance: ₹{liability.OutstandingBalance.Amount:N2}");
            }
            catch (InvalidOperationException ex)
            {
                Logger.LogWarning(ex, "Payment validation failed for liability: {Id}", request.LiabilityId);
                return ApiResponse<LiabilityDto>.ErrorResult(ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error recording payment for liability: {Id}", request.LiabilityId);
                return ApiResponse<LiabilityDto>.ErrorResult($"An error occurred while recording the payment: {ex.Message}");
            }
        }
    }
}
