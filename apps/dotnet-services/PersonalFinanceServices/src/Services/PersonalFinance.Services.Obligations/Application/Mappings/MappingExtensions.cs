using AutoMapper;

using PersonalFinance.Services.Obligations.Application.DataTransferObjects;
using PersonalFinance.Services.Obligations.Domain.Entities;

namespace PersonalFinance.Services.Obligations.Application.Mappings
{
    public static class MappingExtensions
    {
        public static LiabilityDto ToDto(this Liability liability, IMapper mapper)
        {
            var dto = mapper.Map<LiabilityDto>(liability);
            
            // Replicate loan progress logic locally using domain calculation
            var principal = liability.PrincipalAmount.Amount;
            
            if (principal <= 0) 
            {
                dto.EffectiveOutstanding = 0;
                dto.PaidAmount = 0;
                dto.PaidPercent = 0;
                return dto;
            }

            var now = DateTime.UtcNow;
            var elapsed = (now.Year - liability.StartDate.Year) * 12 + (now.Month - liability.StartDate.Month);
            
            decimal theoretical = principal;
            
            if (elapsed > 0)
            {
                var cappedElapsed = Math.Min(elapsed, liability.TenureMonths);
                var emi = liability.EmiAmount.Amount;
                var monthlyRate = liability.InterestRate / 12 / 100;
                
                decimal outstanding = principal;
                for (int i = 0; i < cappedElapsed; i++)
                {
                    decimal interest = liability.InterestRate == 0 ? 0 : Math.Round(outstanding * monthlyRate, 2);
                    decimal principalComp = emi - interest;
                    outstanding -= principalComp;
                    if (outstanding <= 0) 
                    {
                        outstanding = 0;
                        break;
                    }
                }
                theoretical = Math.Round(Math.Max(outstanding, 0), 2);
            }

            dto.EffectiveOutstanding = Math.Min(theoretical, liability.OutstandingBalance.Amount);
            dto.PaidAmount = principal - dto.EffectiveOutstanding;
            dto.PaidPercent = (int)Math.Min(Math.Round((dto.PaidAmount / principal) * 100), 100);

            return dto;
        }

        public static SubscriptionDto ToDto(this Subscription subscription, IMapper mapper)
        {
            return mapper.Map<SubscriptionDto>(subscription);
        }
    }
}
