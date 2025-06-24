using AutoMapper;
using PersonalFinance.Services.Accounts.Application.DTOs;
using PersonalFinance.Services.Accounts.Domain.Entities;

namespace PersonalFinance.Services.Accounts.Application.Mappings
{
    public class AccountMappingProfile : Profile
    {
        public AccountMappingProfile()
        {
            // Account entity <-> AccountTransferObject mapping
            CreateMap<Account, AccountTransferObject>()
                .ForMember(dest => dest.Balance, opt => opt.MapFrom(src => src.Balance));
        }
    }
}