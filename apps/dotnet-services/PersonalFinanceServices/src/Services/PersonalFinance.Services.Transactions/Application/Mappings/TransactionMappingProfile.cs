using AutoMapper;
using PersonalFinance.Services.Transactions.Application.DataTransferObjects;
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
            CreateMap<Transaction, TransactionTransferObject>();
            CreateMap<CreateIncomeTransactionRequest, Transaction>();
            CreateMap<CreateExpenseTransactionRequest, Transaction>();
            CreateMap<CreateTransferTransactionRequest, Transaction>();
        }
    }
}