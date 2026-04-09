using AutoMapper;
using MediatR;
using PersonalFinance.Services.Obligations.Application.Common;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Obligations.Application.Mappings;
using PersonalFinance.Services.Obligations.Domain.Entities;
using PersonalFinance.Services.Obligations.Infrastructure.Data;

namespace PersonalFinance.Services.Obligations.Application.Commands
{
    public class CreateSubscriptionCommand : IRequest<ApiResponse<SubscriptionDto>>
    {
        public string Name { get; set; } = string.Empty;
        public SubscriptionType Type { get; set; }
        public string Provider { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public BillingCycle BillingCycle { get; set; }
        public DateTime StartDate { get; set; }
        public Guid UserId { get; set; }
        public bool AutoRenew { get; set; }
        public DateTime? EndDate { get; set; }

        public CreateSubscriptionCommand(string name, SubscriptionType type, string provider,
            decimal amount, BillingCycle billingCycle, DateTime startDate,
            Guid userId, bool autoRenew, DateTime? endDate)
        {
            Name = name;
            Type = type;
            Provider = provider;
            Amount = amount;
            BillingCycle = billingCycle;
            StartDate = startDate;
            UserId = userId;
            AutoRenew = autoRenew;
            EndDate = endDate;
        }
    }

    public class CreateSubscriptionCommandHandler : BaseRequestHandler<CreateSubscriptionCommand, ApiResponse<SubscriptionDto>>
    {
        public CreateSubscriptionCommandHandler(
            ObligationDbContext context,
            ILogger<CreateSubscriptionCommandHandler> logger,
            IMapper mapper) : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<SubscriptionDto>> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation("Creating subscription: {Name} for user: {UserId}", request.Name, request.UserId);

                var subscription = new Subscription(
                    request.Name,
                    request.Type,
                    request.Provider,
                    request.Amount,
                    request.BillingCycle,
                    request.StartDate,
                    request.UserId,
                    request.AutoRenew,
                    request.EndDate);

                Context.Subscriptions.Add(subscription);
                await Context.SaveChangesAsync(cancellationToken);

                var dto = subscription.ToDto(Mapper);

                Logger.LogInformation("Subscription created successfully: {Id}", subscription.Id);

                return ApiResponse<SubscriptionDto>.SuccessResult(dto, "Subscription created successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating subscription: {Name}", request.Name);
                return ApiResponse<SubscriptionDto>.ErrorResult($"An error occurred while creating the subscription: {ex.Message}");
            }
        }
    }
}
