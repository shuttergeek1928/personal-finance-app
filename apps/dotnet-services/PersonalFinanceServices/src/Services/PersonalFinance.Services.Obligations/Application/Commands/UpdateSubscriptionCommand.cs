using AutoMapper;

using MediatR;

using Microsoft.EntityFrameworkCore;

using PersonalFinance.Services.Obligations.Application.Common;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Obligations.Application.Mappings;
using PersonalFinance.Services.Obligations.Domain.Entities;
using PersonalFinance.Services.Obligations.Infrastructure.Data;

namespace PersonalFinance.Services.Obligations.Application.Commands
{
    public class UpdateSubscriptionCommand : IRequest<ApiResponse<SubscriptionDto>>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public SubscriptionType Type { get; set; }
        public string Provider { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public BillingCycle BillingCycle { get; set; }
        public DateTime StartDate { get; set; }
        public bool AutoRenew { get; set; }
        public DateTime? EndDate { get; set; }

        public UpdateSubscriptionCommand(Guid id, string name, SubscriptionType type, string provider,
            decimal amount, BillingCycle billingCycle, DateTime startDate, bool autoRenew, DateTime? endDate)
        {
            Id = id;
            Name = name;
            Type = type;
            Provider = provider;
            Amount = amount;
            BillingCycle = billingCycle;
            StartDate = startDate;
            AutoRenew = autoRenew;
            EndDate = endDate;
        }
    }

    public class UpdateSubscriptionCommandHandler : BaseRequestHandler<UpdateSubscriptionCommand, ApiResponse<SubscriptionDto>>
    {
        public UpdateSubscriptionCommandHandler(
            ObligationDbContext context,
            ILogger<UpdateSubscriptionCommandHandler> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<SubscriptionDto>> Handle(UpdateSubscriptionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var subscription = await Context.Subscriptions.FirstOrDefaultAsync(s => s.Id == request.Id && s.IsActive, cancellationToken);

                if (subscription == null)
                {
                    Logger.LogWarning("Subscription not found: {Id}", request.Id);
                    return ApiResponse<SubscriptionDto>.ErrorResult("Subscription not found");
                }

                subscription.Update(
                    request.Name,
                    request.Type,
                    request.Provider,
                    request.Amount,
                    request.BillingCycle,
                    request.StartDate,
                    request.AutoRenew,
                    request.EndDate);

                await Context.SaveChangesAsync(cancellationToken);

                var dto = subscription.ToDto(Mapper);

                Logger.LogInformation("Subscription updated successfully: {Id}", subscription.Id);

                return ApiResponse<SubscriptionDto>.SuccessResult(dto, "Subscription updated successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating subscription: {Id}", request.Id);
                return ApiResponse<SubscriptionDto>.ErrorResult($"An error occurred while updating the subscription: {ex.Message}");
            }
        }
    }
}
