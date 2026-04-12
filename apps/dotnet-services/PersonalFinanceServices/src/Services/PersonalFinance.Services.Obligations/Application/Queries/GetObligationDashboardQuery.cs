using AutoMapper;

using MediatR;

using Microsoft.EntityFrameworkCore;

using PersonalFinance.Services.Obligations.Application.Common;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Obligations.Application.Mappings;
using PersonalFinance.Services.Obligations.Domain.Entities;
using PersonalFinance.Services.Obligations.Infrastructure.Data;

namespace PersonalFinance.Services.Obligations.Application.Queries
{
    public class GetObligationDashboardQuery : IRequest<ApiResponse<ObligationDashboardDto>>
    {
        public Guid UserId { get; set; }

        public GetObligationDashboardQuery(Guid userId)
        {
            UserId = userId;
        }
    }

    public class GetObligationDashboardQueryHandler : BaseQueryHandler<GetObligationDashboardQuery, ApiResponse<ObligationDashboardDto>>
    {
        public GetObligationDashboardQueryHandler(ObligationDbContext context, ILogger<GetObligationDashboardQueryHandler> logger, IMapper mapper)
            : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<ObligationDashboardDto>> Handle(GetObligationDashboardQuery request, CancellationToken cancellationToken)
        {
            var liabilities = await Context.Liabilities
                .Include(l => l.CreditCard)
                .Where(l => l.UserId == request.UserId && l.IsActive)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync(cancellationToken);

            var subscriptions = await Context.Subscriptions
                .Where(s => s.UserId == request.UserId && s.IsActive)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(cancellationToken);

            var liabilityDtos = liabilities.Select(l => l.ToDto(Mapper)).ToList();
            var subscriptionDtos = subscriptions.Select(s => s.ToDto(Mapper)).ToList();

            var totalMonthlyEmi = liabilities.Sum(l => l.EmiAmount.Amount);
            var totalOutstanding = liabilities.Sum(l => l.OutstandingBalance.Amount);

            // Normalize subscription costs to monthly
            var totalMonthlySubscription = subscriptions.Sum(s => s.BillingCycle switch
            {
                BillingCycle.Monthly => s.Amount.Amount,
                BillingCycle.Quarterly => s.Amount.Amount / 3,
                BillingCycle.HalfYearly => s.Amount.Amount / 6,
                BillingCycle.Yearly => s.Amount.Amount / 12,
                _ => s.Amount.Amount
            });

            var dashboard = new ObligationDashboardDto
            {
                TotalActiveLiabilities = liabilities.Count,
                TotalOutstandingBalance = totalOutstanding,
                TotalMonthlyEmi = totalMonthlyEmi,
                TotalActiveSubscriptions = subscriptions.Count,
                TotalMonthlySubscriptionCost = Math.Round(totalMonthlySubscription, 2),
                TotalMonthlyObligations = Math.Round(totalMonthlyEmi + totalMonthlySubscription, 2),
                Liabilities = liabilityDtos,
                Subscriptions = subscriptionDtos
            };

            return ApiResponse<ObligationDashboardDto>.SuccessResult(dashboard);
        }
    }
}
