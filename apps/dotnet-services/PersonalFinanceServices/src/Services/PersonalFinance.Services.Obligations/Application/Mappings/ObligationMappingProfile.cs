using AutoMapper;
using PersonalFinance.Services.Obligations.Application.DataTransferObjects;
using PersonalFinance.Services.Obligations.Domain.Entities;

namespace PersonalFinance.Services.Obligations.Application.Mappings
{
    public class ObligationMappingProfile : Profile
    {
        public ObligationMappingProfile()
        {
            CreateMap<Liability, LiabilityDto>()
                .ForMember(dest => dest.PrincipalAmount, opt => opt.MapFrom(src => src.PrincipalAmount))
                .ForMember(dest => dest.OutstandingBalance, opt => opt.MapFrom(src => src.OutstandingBalance))
                .ForMember(dest => dest.EmiAmount, opt => opt.MapFrom(src => src.EmiAmount))
                .ForMember(dest => dest.CreditCard, opt => opt.MapFrom(src => src.CreditCard))
                .ForMember(dest => dest.CreditCardId, opt => opt.MapFrom(src => src.CreditCardId))
                .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.AccountId));

            // Subscription entity <-> SubscriptionDto mapping
            CreateMap<Subscription, SubscriptionDto>()
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount));

            // CreditCard entity <-> CreditCardDto mapping
            CreateMap<CreditCard, CreditCardDto>();
        }
    }
}
