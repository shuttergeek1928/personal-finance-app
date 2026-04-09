using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.Services.Obligations.Application.Common;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects.Response;
using PersonalFinance.Services.Obligations.Domain.Entities;
using PersonalFinance.Services.Obligations.Infrastructure.Data;

namespace PersonalFinance.Services.Obligations.Application.Queries
{
    public class GetAmortizationScheduleQuery : IRequest<ApiResponse<AmortizationScheduleDto>>
    {
        public Guid LiabilityId { get; set; }

        public GetAmortizationScheduleQuery(Guid liabilityId)
        {
            LiabilityId = liabilityId;
        }
    }

    public class GetAmortizationScheduleQueryHandler : BaseQueryHandler<GetAmortizationScheduleQuery, ApiResponse<AmortizationScheduleDto>>
    {
        public GetAmortizationScheduleQueryHandler(ObligationDbContext context, ILogger<GetAmortizationScheduleQueryHandler> logger, IMapper mapper)
            : base(context, logger, mapper)
        {
        }

        public override async Task<ApiResponse<AmortizationScheduleDto>> Handle(GetAmortizationScheduleQuery request, CancellationToken cancellationToken)
        {
            var liability = await Context.Liabilities
                .FirstOrDefaultAsync(l => l.Id == request.LiabilityId && l.IsActive, cancellationToken);

            if (liability == null)
            {
                Logger.LogWarning("Liability not found: {Id}", request.LiabilityId);
                return ApiResponse<AmortizationScheduleDto>.ErrorResult("Liability not found");
            }

            var schedule = ComputeAmortizationSchedule(liability);
            return ApiResponse<AmortizationScheduleDto>.SuccessResult(schedule);
        }

        /// <summary>
        /// Computes the full amortization schedule using the reducing balance method.
        /// EMI = P × r × (1+r)^n / ((1+r)^n - 1)
        /// Each month: Interest = Outstanding × r, Principal = EMI - Interest
        /// </summary>
        private static AmortizationScheduleDto ComputeAmortizationSchedule(Liability liability)
        {
            var principal = liability.PrincipalAmount.Amount;
            var annualRate = liability.InterestRate;
            var tenureMonths = liability.TenureMonths;
            var startDate = liability.StartDate;

            var monthlyRate = annualRate / 12 / 100;
            var emi = Liability.CalculateEmi(principal, annualRate, tenureMonths);

            var schedule = new AmortizationScheduleDto
            {
                LiabilityId = liability.Id,
                LiabilityName = liability.Name,
                MonthlyEmi = emi,
                Schedule = new List<AmortizationScheduleItemDto>()
            };

            var outstanding = principal;
            var totalInterest = 0m;

            for (int month = 1; month <= tenureMonths; month++)
            {
                var interestComponent = Math.Round(outstanding * monthlyRate, 2);
                var principalComponent = Math.Round(emi - interestComponent, 2);

                // Handle last month rounding
                if (month == tenureMonths)
                {
                    principalComponent = outstanding;
                    interestComponent = emi - principalComponent;
                    if (interestComponent < 0) interestComponent = 0;
                }

                outstanding = Math.Max(outstanding - principalComponent, 0);
                totalInterest += interestComponent;

                schedule.Schedule.Add(new AmortizationScheduleItemDto
                {
                    Month = month,
                    PaymentDate = startDate.AddMonths(month - 1),
                    EmiAmount = emi,
                    PrincipalComponent = principalComponent,
                    InterestComponent = interestComponent,
                    OutstandingBalance = Math.Round(outstanding, 2)
                });
            }

            schedule.TotalAmountPayable = Math.Round(emi * tenureMonths, 2);
            schedule.TotalInterestPayable = Math.Round(totalInterest, 2);

            return schedule;
        }
    }
}
