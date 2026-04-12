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
    public class GetSubscriptionByIdQuery : IRequest<ApiResponse<SubscriptionDto>>
    {
        public Guid Id { get; set; }

        public GetSubscriptionByIdQuery(Guid id)
        {
            Id = id;
        }
    }

    public class GetSubscriptionByIdQueryHandler : BaseQueryHandler<GetSubscriptionByIdQuery, ApiResponse<SubscriptionDto>>
    {
        public GetSubscriptionByIdQueryHandler(ObligationDbContext context, ILogger<GetSubscriptionByIdQueryHandler> logger, IMapper mapper)
            : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<SubscriptionDto>> Handle(GetSubscriptionByIdQuery request, CancellationToken cancellationToken)
        {
            var subscription = await Context.Subscriptions
                .FirstOrDefaultAsync(s => s.Id == request.Id && s.IsActive, cancellationToken);

            if (subscription == null)
            {
                Logger.LogWarning("Subscription not found: {Id}", request.Id);
                return ApiResponse<SubscriptionDto>.ErrorResult("Subscription not found");
            }

            var dto = subscription.ToDto(Mapper);
            return ApiResponse<SubscriptionDto>.SuccessResult(dto);
        }
    }
}
