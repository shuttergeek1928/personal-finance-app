using AutoMapper;

using PersonalFinance.Services.Transactions.Application.DataTransferObjects.Requests;
using PersonalFinance.Services.Transactions.Application.DTOs;
using PersonalFinance.Services.Transactions.Domain.Entities;

namespace PersonalFinance.Services.Transactions.Application.Mappings
{
    public class TransactionMappingProfile : Profile
    {
        public TransactionMappingProfile()
        {
            // Transaction entity <-> User DTO
            CreateMap<Transaction, TransactionTransferObject>()
                .ForMember(dest => dest.CreditCardId, opt => opt.MapFrom(src => src.CreditCardId))
                .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.AccountId));
            CreateMap<CreateIncomeTransactionRequest, Transaction>();
            CreateMap<CreateExpenseTransactionRequest, Transaction>();
            CreateMap<CreateTransferTransactionRequest, Transaction>();
        }
    }
}