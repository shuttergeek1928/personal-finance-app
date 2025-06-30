using AutoMapper;
using PersonalFinance.Services.Transactions.Application.DataTransferObjects;
using PersonalFinance.Services.Transactions.Application.DTOs;
using PersonalFinance.Services.Transactions.Domain.Entities;

namespace PersonalFinance.Services.Transactions.Application.Mappings
{
    public static class MappingExtensions
    {
        // Extension methods for common mapping scenarios
        public static TransactionTransferObject ToDto(this Transaction user, IMapper mapper)
        {
            return mapper.Map<TransactionTransferObject>(user);
        }
    }
}
