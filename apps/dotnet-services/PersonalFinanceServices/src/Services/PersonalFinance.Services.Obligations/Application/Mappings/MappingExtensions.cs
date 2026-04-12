using AutoMapper;

using PersonalFinance.Services.Obligations.Application.DataTransferObjects;
using PersonalFinance.Services.Obligations.Domain.Entities;

namespace PersonalFinance.Services.Obligations.Application.Mappings
{
    public static class MappingExtensions
    {
        public static LiabilityDto ToDto(this Liability liability, IMapper mapper)
        {
            return mapper.Map<LiabilityDto>(liability);
        }

        public static SubscriptionDto ToDto(this Subscription subscription, IMapper mapper)
        {
            return mapper.Map<SubscriptionDto>(subscription);
        }
    }
}
