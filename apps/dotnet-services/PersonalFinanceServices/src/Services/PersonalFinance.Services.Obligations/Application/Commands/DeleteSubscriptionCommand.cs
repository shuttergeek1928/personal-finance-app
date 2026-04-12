using AutoMapper;

using MediatR;

using Microsoft.EntityFrameworkCore;

using PersonalFinance.Services.Obligations.Application.Common;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Obligations.Application.Mappings;
using PersonalFinance.Services.Obligations.Infrastructure.Data;

namespace PersonalFinance.Services.Obligations.Application.Commands
{
    public class DeleteSubscriptionCommand : IRequest<ApiResponse<SubscriptionDto>>
    {
        public Guid Id { get; set; }

        public DeleteSubscriptionCommand(Guid id)
        {
            Id = id;
        }
    }

    public class DeleteSubscriptionCommandHandler : BaseRequestHandler<DeleteSubscriptionCommand, ApiResponse<SubscriptionDto>>
    {
        public DeleteSubscriptionCommandHandler(
            ObligationDbContext context,
            ILogger<DeleteSubscriptionCommandHandler> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<SubscriptionDto>> Handle(DeleteSubscriptionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var subscription = await Context.Subscriptions.FirstOrDefaultAsync(s => s.Id == request.Id && s.IsActive, cancellationToken);

                if (subscription == null)
                {
                    Logger.LogWarning("Subscription not found: {Id}", request.Id);
                    return ApiResponse<SubscriptionDto>.ErrorResult("Subscription not found");
                }

                // Soft delete
                subscription.IsActive = false;
                await Context.SaveChangesAsync(cancellationToken);

                var dto = subscription.ToDto(Mapper);

                Logger.LogInformation("Subscription deleted successfully: {Id}", request.Id);

                return ApiResponse<SubscriptionDto>.SuccessResult(dto, "Subscription deleted successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error deleting subscription: {Id}", request.Id);
                return ApiResponse<SubscriptionDto>.ErrorResult($"An error occurred while deleting the subscription: {ex.Message}");
            }
        }
    }
}
