using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.Services.Obligations.Application.Common;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Obligations.Application.Mappings;
using PersonalFinance.Services.Obligations.Infrastructure.Data;

namespace PersonalFinance.Services.Obligations.Application.Queries
{
    public class GetSubscriptionsByUserIdQuery : IRequest<ApiResponse<List<SubscriptionDto>>>
    {
        public Guid UserId { get; set; }

        public GetSubscriptionsByUserIdQuery(Guid userId)
        {
            UserId = userId;
        }
    }

    public class GetSubscriptionsByUserIdQueryHandler : BaseQueryHandler<GetSubscriptionsByUserIdQuery, ApiResponse<List<SubscriptionDto>>>
    {
        public GetSubscriptionsByUserIdQueryHandler(ObligationDbContext context, ILogger<GetSubscriptionsByUserIdQueryHandler> logger, IMapper mapper)
            : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<List<SubscriptionDto>>> Handle(GetSubscriptionsByUserIdQuery request, CancellationToken cancellationToken)
        {
            var subscriptions = await Context.Subscriptions
                .Where(s => s.UserId == request.UserId && s.IsActive)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(cancellationToken);

            if (!subscriptions.Any())
            {
                Logger.LogInformation("No subscriptions found for user: {UserId}", request.UserId);
                return ApiResponse<List<SubscriptionDto>>.SuccessResult(new List<SubscriptionDto>(), "No subscriptions found");
            }

            var dtos = subscriptions.Select(s => s.ToDto(Mapper)).ToList();
            return ApiResponse<List<SubscriptionDto>>.SuccessResult(dtos);
        }
    }
}
