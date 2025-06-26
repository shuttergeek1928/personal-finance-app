using AutoMapper;
using PersonalFinance.Services.Accounts.Application.DTOs;
using PersonalFinance.Services.Accounts.Domain.Entities;

namespace PersonalFinance.Services.Accounts.Application.Mappings
{
    public static class MappingExtensions
    {
        // Extension methods for common mapping scenarios
        public static AccountTransferObject ToDto(this Account account, IMapper mapper)
        {
            return mapper.Map<AccountTransferObject>(account);
        }
    }
}
